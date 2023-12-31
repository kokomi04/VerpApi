﻿using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using static Verp.Resources.Stock.Product.ProductValidationMessage;
using static VErp.Commons.GlobalObject.InternalDataInterface.Stock.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement.ProductMaterialsConsumptionFacade
{
    public class ProductMaterialsConsumptionImportFacade
    {
        private ObjectActivityLogFacade _productActivityLog;

        private StockDBContext _stockDbContext;

        private IOrganizationHelperService _organizationHelperService;
        private IManufacturingHelperService _manufacturingHelperService;
        //private IActivityLogService _activityLogService;

        private IProductBomService _productBomService;
        private IUnitService _unitService;
        private IProductService _productService;
        private IProductMaterialsConsumptionService _productMaterialsConsumptionService;
        private ICustomGenCodeHelperService _customGenCodeHelperService;

        private IList<ImportProductMaterialsConsumptionExcelMapping> _importData;
        private IDictionary<string, UnitOutput> _units;
        private IDictionary<string, SimpleProduct> _existedProducts;
        private IDictionary<string, ProductType> _productTypes;
        private IDictionary<string, ProductCate> _productCates;

        private IList<ProductBomOutput> _boms;
        private ProductModel _productInfo;
        private Dictionary<string, ProductMaterialsConsumptionGroup> _groupConsumptions;
        private IList<DepartmentSimpleModel> _departments;
        private Dictionary<string, StepSimpleInfo> _steps;

        private bool IsPreview;

        public IList<MaterialsConsumptionByProduct> PreviewData { get; private set; }

        public ProductMaterialsConsumptionImportFacade(bool isPreview)
        {
            IsPreview = isPreview;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IUnitService unitService)
        {
            _unitService = unitService;
            return this;
        }
        public ProductMaterialsConsumptionImportFacade SetService(IProductService productService)
        {
            _productService = productService;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IProductBomService productBomService)
        {
            _productBomService = productBomService;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IOrganizationHelperService organizationHelperService)
        {
            _organizationHelperService = organizationHelperService;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IManufacturingHelperService manufacturingHelperService)
        {
            _manufacturingHelperService = manufacturingHelperService;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IActivityLogService activityLogService)
        {
            //_activityLogService = activityLogService;
            _productActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Product);
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(IProductMaterialsConsumptionService productMaterialsConsumptionService)
        {
            _productMaterialsConsumptionService = productMaterialsConsumptionService;
            return this;
        }

        public ProductMaterialsConsumptionImportFacade SetService(ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _customGenCodeHelperService = customGenCodeHelperService;
            return this;
        }

        private ImportExcelMapping _mapping = null;
        public async Task<bool> ProcessData(ILongTaskResourceLockService longTaskResourceLockService, ImportExcelMapping mapping, Stream stream, int? productId)
        {
            _mapping = mapping;
            using (var longTask = await longTaskResourceLockService.Accquire($"Nhập dữ liệu vật tư tiêu hao từ excel"))
            {
                ReadExcelData(longTask, mapping, stream);

                longTask.SetTotalRows(_importData.Count);

                if (productId.HasValue)
                    await ValidWithProductBOM(productId);

                await ValidExcelData();
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    using (var logBath = _productActivityLog.BeginBatchLog())
                    {
                        longTask.SetCurrentStep("Thêm loại mặt hàng");
                        await AddMissingProductType();

                        longTask.SetCurrentStep("Thêm danh mục mặt hàng");
                        await AddMissingProductCate();

                        longTask.SetCurrentStep("Thêm đơn vị tính");
                        await AddMissingUnit();

                        longTask.SetCurrentStep("Thêm mặt hàng");

                        await AddMissingProduct();

                        longTask.SetCurrentStep("Thêm nhóm vật tư tiêu hao");
                        await AddMissingMaterialConsumptionGroup();

                        if (!IsPreview)
                        {

                            await ImportProcess(longTask);

                            await trans.CommitAsync();
                            await logBath.CommitAsync();
                        }
                        else
                        {
                            PreviewData = new List<MaterialsConsumptionByProduct>();
                            var allUsageProductCode = _importData.Select(x => x.UsageProductCode).Distinct();
                            var allProductBoms = await _productBomService.GetBoms(_existedProducts.Values.Where(x => allUsageProductCode.Contains(x.ProductCode) && x.ProductId > 0).Select(x => x.ProductId).ToArray());

                            await LoadPreviewData(allProductBoms, longTask);
                        }

                    }
                }
                return true;
            }
        }

        private async Task ValidWithProductBOM(int? productId)
        {
            _boms = await _productBomService.GetBom(productId.Value);
            _productInfo = await _productService.ProductInfo(productId.Value);

            var notExistsUsageProductInBom = _importData.Where(x => x.UsageProductCode != _productInfo.ProductCode)
                                                        .Where(x => !_boms.Any(b => b.ProductCode == x.UsageProductCode))
                                                        .Select(x => x.UsageProductCode).ToList();
            if (notExistsUsageProductInBom.Count() > 0)
                throw ImportConsumBomNotExistedInProduct.BadRequestFormat(string.Join(", ", notExistsUsageProductInBom));

        }

        private async Task ValidExcelData()
        {

            var groupProductByConsumGroups = _importData
                .Where(x => !string.IsNullOrWhiteSpace(x.UsageProductCode))
                .GroupBy(x => new { x.UsageProductCode, GroupTitle = x.GroupTitle.NormalizeAsInternalName() })
                .ToList();
            foreach (var consumGroupProduct in groupProductByConsumGroups)
            {
                var groupMaterial = consumGroupProduct.GroupBy(x => x.ProductCode).ToList();
                foreach (var material in groupMaterial)
                {
                    if (material.Count() > 1)
                    {
                        var marterialCode = material.First().ProductCode;
                        var groupTitle = consumGroupProduct.First().GroupTitle;
                        var partOfProduct = consumGroupProduct.Key.UsageProductCode;
                        var excelRows = material.Select(m => m.RowNumber).ToArray();
                        var excelRowsStr = string.Join(", ", excelRows);
                        throw ImportConsumDuplicateSamePartInSameGroup.BadRequestFormat(marterialCode, groupTitle, partOfProduct, excelRowsStr);
                    }
                }
            }

            _departments = await _organizationHelperService.GetAllDepartmentSimples();
            _steps = (await _manufacturingHelperService.GetSteps()).GroupBy(x => x.StepName.NormalizeAsInternalName())
                                                                                       .ToDictionary(k => k.Key, v => v.First());

            foreach (var row in _importData)
            {
                if (string.IsNullOrWhiteSpace(row.DepartmentName) && string.IsNullOrWhiteSpace(row.DepartmentCode))
                    continue;

                var department = _departments.FirstOrDefault(x => x.DepartmentCode == row.DepartmentCode || x.DepartmentName == row.DepartmentName);
                if (department == null)
                    throw DepartmentOfMaterialNotFound.BadRequestFormat($"{row.DepartmentCode} {row.DepartmentName}", row.ProductCode);

                if (string.IsNullOrWhiteSpace(row.StepName))
                    continue;

                if (!_steps.ContainsKey(row.StepName.NormalizeAsInternalName()))
                    throw StepOfMaterialNotFound.BadRequestFormat(row.StepName, row.ProductCode);
            }
        }

        private void ReadExcelData(LongTaskResourceLock longTask, ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            reader.RegisterLongTaskEvent(longTask);
            _importData = reader.ReadSheetEntity<ImportProductMaterialsConsumptionExcelMapping>(mapping);
        }
        private async Task<ImportProductMaterialsConsumptionExcelMapping> ValidateProductMaterial(IList<ImportProductMaterialsConsumptionExcelMapping> importProducts, IDictionary<string, int> baseValueChains, List<IGenerateCodeContext> ctxs, ImportProductMaterialsConsumptionExcelMapping productMaterial, bool isValidateSpecification = false)
        {
           
            // validateProducts
            if (string.IsNullOrEmpty(productMaterial.ProductCode))
            {
                var productCodesExis = importProducts.Where(x => x.ProductName == productMaterial.ProductName && x.Specification == productMaterial.Specification && !string.IsNullOrEmpty(x.ProductCode)).Select(x => x.ProductCode).ToList();
                productCodesExis.AddRange(importProducts.Where(x => x.UsageProductName == productMaterial.ProductName && x.UsageSpecification == productMaterial.Specification && !string.IsNullOrEmpty(x.UsageProductCode)).Select(x => x.UsageProductCode));
                if (productCodesExis.Count > 0)
                {
                    productMaterial.ProductCode = productCodesExis.FirstOrDefault();
                }
                else
                {
                    var productCodes = isValidateSpecification ? _existedProducts.Where(x => x.Value.ProductName == productMaterial.ProductName && x.Value.Specification == productMaterial.Specification).Select(x => x.Value.ProductCode).ToList()
                  : _existedProducts.Where(x => x.Value.ProductName == productMaterial.ProductName).Select(x => x.Value.ProductCode).ToList();

                    if (productCodes.Count > 1)
                    {
                        if (isValidateSpecification)
                            throw new BadRequestException($"Có nhiều mặt hàng giống tên: {productMaterial.ProductName} và quy cách: {productMaterial.Specification}");
                        else throw new BadRequestException($"Có nhiều mặt hàng giống tên: {productMaterial.ProductName}");

                    }
                    if (productCodes.Count == 0 && string.IsNullOrEmpty(productMaterial.ProductCode))
                    {
                        var (ctx, code) = await CustomGenCode(productMaterial, baseValueChains);
                        productMaterial.ProductCode = code;
                    }
                    if (!string.IsNullOrEmpty(productCodes.FirstOrDefault()))
                    {
                        productMaterial.ProductCode = productCodes.FirstOrDefault();
                    }
                }
               
            }
            // Validate ChildProducts
            if (string.IsNullOrEmpty(productMaterial.UsageProductCode))
            {

                var childProductCodesExis = importProducts.Where(x => x.UsageProductName == productMaterial.UsageProductName && x.UsageSpecification == productMaterial.UsageSpecification && !string.IsNullOrEmpty(x.UsageProductCode)).Select(x => x.UsageProductCode).ToList();
                childProductCodesExis.AddRange(importProducts.Where(x => x.ProductName == productMaterial.UsageProductName && x.Specification == productMaterial.UsageSpecification && !string.IsNullOrEmpty(x.ProductCode)).Select(x => x.ProductCode));
                if (childProductCodesExis.Count > 0)
                {
                    productMaterial.UsageProductCode = childProductCodesExis.FirstOrDefault();
                    return productMaterial;
                }
                else
                {
                    var childProductCodes = isValidateSpecification ? _existedProducts.Where(x => x.Value.ProductName == productMaterial.UsageProductName && x.Value.Specification == productMaterial.UsageSpecification).Select(x => x.Value.ProductCode).ToList()
                    : _existedProducts.Where(x => x.Value.ProductName == productMaterial.UsageProductName).Select(x => x.Value.ProductCode).ToList(); ;

                    if (childProductCodes.Count > 1)
                    {
                        if (isValidateSpecification)
                            throw new BadRequestException($"Có nhiều chi tiết giống tên: {productMaterial.UsageProductName} và quy cách: {productMaterial.UsageProductCode}");
                        else throw new BadRequestException($"Có nhiều chi tiết giống tên: {productMaterial.UsageProductName}");
                    }
                    if (childProductCodes.Count == 0 && string.IsNullOrEmpty(productMaterial.UsageProductCode))
                    {
                        var (ctx, code) = await CustomGenCode(productMaterial, baseValueChains, true);
                        productMaterial.UsageProductCode = code;
                    }
                    if (!string.IsNullOrEmpty(childProductCodes.FirstOrDefault()))
                    {
                        productMaterial.UsageProductCode = childProductCodes.FirstOrDefault();
                    }
                }
                
            }
            return productMaterial;
        }
        private async Task<(IGenerateCodeContext, string)> CustomGenCode(ImportProductMaterialsConsumptionExcelMapping productMaterial, IDictionary<string, int> baseValueChains = null, bool isChildProduct = false)
        {
            ProductType type = null;
            if (!isChildProduct)
            {
                if (string.IsNullOrWhiteSpace(productMaterial.ProductTypeCode.NormalizeAsInternalName()))
                {
                    type = _productTypes.FirstOrDefault(c => c.Value.IsDefault).Value;
                }
                else
                {
                    _productTypes.TryGetValue(productMaterial.ProductTypeCode.NormalizeAsInternalName(), out type);
                    if (type == null)
                    {
                        throw ImportProductCateOfProductNotFound.BadRequestFormat(productMaterial.ProductTypeCode, productMaterial.ProductName);
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(productMaterial.UsageProductTypeCode.NormalizeAsInternalName()))
                {
                    type = _productTypes.FirstOrDefault(c => c.Value.IsDefault).Value;
                }
                else
                {
                    _productTypes.TryGetValue(productMaterial.UsageProductTypeCode.NormalizeAsInternalName(), out type);
                    if (type == null)
                    {
                        throw ImportProductCateOfProductNotFound.BadRequestFormat(productMaterial.UsageProductTypeCode, productMaterial.UsageProductName);
                    }
                }
            }

            var productTypeInfo = await _stockDbContext.ProductType.FirstOrDefaultAsync(t => t.ProductTypeId == type.ProductTypeId);

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
            var code = await ctx
                .SetConfig(EnumObjectType.Product, EnumObjectType.ProductType, type.ProductTypeId, productTypeInfo?.ProductTypeName)
                .SetConfigData(0, null, productTypeInfo?.IdentityCode)
                .TryValidateAndGenerateCode(_stockDbContext.Product, isChildProduct ? productMaterial.UsageProductCode : productMaterial.ProductCode, (s, code) => s.ProductId != 0 && s.ProductCode == code);
            return (ctx, code);
        }

        private void ValidateWithNameProduct(ImportProductMaterialsConsumptionExcelMapping productMaterial)
        {
            var errorProductNames = _importData.Where(x => (x.ProductName == productMaterial.ProductName && x.Specification != productMaterial.Specification)
            || (x.UsageProductName == productMaterial.ProductName && x.UsageSpecification != productMaterial.Specification)).ToList();
            if (errorProductNames.Count > 0)
            {
                throw new BadRequestException($"Mặt hàng có tên {productMaterial.ProductName} đang có nhiều quy cách khác nhau! Vui lòng kiểm tra lại!");
            }
            var errorChildProduct = _importData.Where(x => (x.ProductName == productMaterial.UsageProductName && x.Specification != productMaterial.UsageProductName)
            || (x.UsageProductName == productMaterial.UsageProductName && x.UsageSpecification != productMaterial.UsageSpecification)).ToList();
            if (errorChildProduct.Count >0)
                throw new BadRequestException($"Chi tiết có tên {productMaterial.UsageProductName} đang có nhiều quy cách khác nhau! Vui lòng kiểm tra lại!");
        }
        private async Task<IList<IGenerateCodeContext>> ValidateProductMaterials()
        {
            var importProducts = new List<ImportProductMaterialsConsumptionExcelMapping>();
            var ctxs = new List<IGenerateCodeContext>();
            var baseValueChains = new Dictionary<string, int>();
            foreach (var productImport in _importData)
            {
                switch (_mapping.HandleFilterOptionId)
                {
                    case EnumHandleFilterOption.Default:
                        if (productImport.ProductCode == null || productImport.UsageProductCode == null)
                        {
                            throw new BadRequestException("Vui lòng nhập mã mặt hàng!");
                        }
                        importProducts.Add(productImport);
                        break;
                    case EnumHandleFilterOption.FitlerByNameAndSpecification:
                        importProducts.Add(await ValidateProductMaterial(importProducts, baseValueChains, ctxs, productImport, true));
                        break;
                    case EnumHandleFilterOption.FilterByName:
                        ValidateWithNameProduct(productImport);
                        importProducts.Add(await ValidateProductMaterial(importProducts, baseValueChains, ctxs, productImport));
                        break;
                    default:
                        break;
                }
            }
            _importData = importProducts;
            return ctxs;
        }
        private async Task ImportProcess(LongTaskResourceLock longTask)
        {
            var data = _importData.GroupBy(x => x.GroupTitle.NormalizeAsInternalName());

            var lsMaterialConsumption = new List<ProductMaterialsConsumption>();

            longTask.SetCurrentStep("Xử lý dữ liệu lưu vào cơ sở dữ liệu");

            longTask.SetTotalRows(data.Count());

            foreach (var d in data)
            {
                if (_groupConsumptions.ContainsKey(d.Key))
                {
                    var groupMaterialConsumptionId = _groupConsumptions[d.Key].ProductMaterialsConsumptionGroupId;

                    var usageProductCodes = d.Select(x => x.UsageProductCode).ToArray();
                    var usageProductIds = _existedProducts.Values.Where(x => usageProductCodes.Contains(x.ProductCode)).Select(x => x.ProductId).ToArray();

                    if (!IsPreview)
                        await RemoveOldMaterialConsumption(usageProductIds, groupMaterialConsumptionId);

                    /* Thêm mới row data cũ */
                    foreach (var (usageProductCode, rows) in d.GroupBy(x => x.UsageProductCode.NormalizeAsInternalName()).ToDictionary(k => k.Key, v => v))
                    {
                        _existedProducts.TryGetValue(usageProductCode, out var rootProduct);

                        var newMaterialConsumptions = from r in rows
                                                      let stepId = GetStep(r.StepName)?.StepId
                                                      let departmentId = GetDepartment(r.DepartmentCode, r.DepartmentName)?.DepartmentId
                                                      let materialsConsumptionId = _existedProducts[r.ProductCode.NormalizeAsInternalName()].ProductId
                                                      select new ProductMaterialsConsumption
                                                      {
                                                          MaterialsConsumptionId = materialsConsumptionId,
                                                          ProductId = rootProduct.ProductId,
                                                          Quantity = r.Quantity,
                                                          Wastage = r.Wastage ?? 1,
                                                          DepartmentId = departmentId,
                                                          StepId = stepId,
                                                          ProductMaterialsConsumptionGroupId = groupMaterialConsumptionId,
                                                          Description = r.Description
                                                      };

                        lsMaterialConsumption.AddRange(newMaterialConsumptions);

                    }


                }
                longTask.IncProcessedRows();
            }

            if (!IsPreview)
            {
                await _stockDbContext.ProductMaterialsConsumption.AddRangeAsync(lsMaterialConsumption);

                var productIds = lsMaterialConsumption.Select(c => c.ProductId).Distinct().ToList();

                var products = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

                await _stockDbContext.SaveChangesAsync();
                foreach (var p in products)
                {
                    var consumps = lsMaterialConsumption.Where(c => c.ProductId == p.ProductId).ToList();
                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.ImportConsumption)
                     .MessageResourceFormatDatas(p.ProductCode)
                     .ObjectId(p.ProductId)
                     .JsonData(new { _mapping, consumps })
                     .CreateLog();
                }

            }
        }

        private async Task LoadPreviewData(IDictionary<int, IList<ProductBomOutput>> allProductBoms, LongTaskResourceLock longTask)
        {
            var allUsageProductCode = _importData.Select(x => x.UsageProductCode.NormalizeAsInternalName()).Distinct();
            var rootProductIds = FoundRelationshipBom(allProductBoms);

            var groupTitle = _importData.Select(x => x.GroupTitle.NormalizeAsInternalName()).Distinct().ToArray();
            var groupIds = _groupConsumptions.Values.Where(x => groupTitle.Contains(x.Title.NormalizeAsInternalName())).Select(x => x.ProductMaterialsConsumptionGroupId).ToArray();

            foreach (var usageProductCode in allUsageProductCode)
            {
                _existedProducts.TryGetValue(usageProductCode, out var rootProduct);
                if (allProductBoms.ContainsKey(rootProduct.ProductId) && rootProductIds.Contains(rootProduct.ProductId))
                {


                    var productBom = (await _productBomService.GetBom(rootProduct.ProductId)).Where(x => !x.IsMaterial);
                    var productMap = CalcProductBomTotalQuantity(productBom);

                    var materialsConsumptionInherit = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                        .Where(x => productBom.Select(y => y.ChildProductId).Contains(x.ProductId) && groupIds.Contains(x.ProductMaterialsConsumptionGroupId))
                        .Include(x => x.MaterialsConsumption)
                        .Include(x => x.ProductMaterialsConsumptionGroup)
                        .Include(x => x.Product)
                        .Select(x => new ProductMaterialsConsumptionPreview
                        {
                            Quantity = x.Quantity,
                            Wastage = x.Wastage,
                            GroupTitle = x.ProductMaterialsConsumptionGroup.Title,
                            ProductExtraInfo = _existedProducts[x.Product.ProductCode.NormalizeAsInternalName()],
                            ProductMaterialsComsumptionExtraInfo = _existedProducts[x.MaterialsConsumption.ProductCode.NormalizeAsInternalName()],
                            Description = x.Description
                        })
                        .ToListAsync());

                    var materialsInherit = LoopGetMaterialConsumInherit(materialsConsumptionInherit, productBom, productBom.Where(x => x.Level == 1), productMap);

                    var materialsConsumption = _importData.Where(x => x.UsageProductCode.NormalizeAsInternalName() == usageProductCode)
                    .Select(x => new ProductMaterialsConsumptionPreview
                    {
                        BomQuantity = 1,
                        DepartmentName = x.DepartmentName ?? x.DepartmentCode,
                        GroupTitle = x.GroupTitle,
                        Quantity = x.Quantity,
                        Wastage = x.Wastage ?? 1,
                        StepName = x.StepName,
                        ProductExtraInfo = _existedProducts[x.UsageProductCode.NormalizeAsInternalName()],
                        ProductMaterialsComsumptionExtraInfo = _existedProducts[x.ProductCode.NormalizeAsInternalName()],
                        Description = x.Description,
                        IsImported = true
                    })
                    .ToList();

                    var exceptMaterialsConsumption = materialsInherit.Except(materialsConsumption, new ProductMaterialsConsumptionPreviewComparer())
                        .Select(x => new ProductMaterialsConsumptionPreview
                        {
                            GroupTitle = x.GroupTitle,
                            DepartmentName = x.DepartmentName,
                            StepName = x.StepName,
                            ProductExtraInfo = rootProduct,
                            ProductMaterialsComsumptionExtraInfo = x.ProductMaterialsComsumptionExtraInfo,
                            Description = x.Description,
                            IsImported = x.IsImported
                        });

                    materialsConsumption.AddRange(exceptMaterialsConsumption);
                    foreach (var m in materialsConsumption)
                    {
                        m.MaterialsConsumptionInherit = materialsInherit.Where(x => x.GroupTitle == m.GroupTitle
                                        && x.ProductMaterialsComsumptionExtraInfo.ProductCode == m.ProductMaterialsComsumptionExtraInfo.ProductCode).ToList();
                        m.BomQuantity = 1;
                        m.TotalQuantityInheritance = m.MaterialsConsumptionInherit.Select(x => (x.Quantity * x.Wastage * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
                    }

                    PreviewData.Add(new MaterialsConsumptionByProduct
                    {
                        RootProduct = rootProduct,
                        MaterialsComsump = materialsConsumption
                    });
                }
                else if (rootProduct.ProductId == 0)
                {
                    var materials = _importData.Where(x => x.UsageProductCode.NormalizeAsInternalName() == usageProductCode)
                    .GroupBy(x => x.GroupTitle.NormalizeAsInternalName())
                    .ToDictionary(k => k.Key, v => v.Select(x => new ProductMaterialsConsumptionPreview
                    {
                        GroupTitle = v.First().GroupTitle,
                        Quantity = x.Quantity,
                        DepartmentName = GetDepartment(x.DepartmentCode, x.DepartmentName)?.DepartmentName,
                        StepName = GetStep(x.StepName)?.StepName,
                        ProductExtraInfo = new SimpleProduct { ProductCode = x.UsageProductCode, ProductName = x.UsageProductName, UnitName = x.UnitName, Specification = x.UsageSpecification },
                        ProductMaterialsComsumptionExtraInfo = new SimpleProduct { ProductCode = x.ProductCode, ProductName = x.ProductName, UnitName = x.UnitName, Specification = x.Specification },
                        Description = x.Description,
                        IsImported = true
                    }))
                    .SelectMany(v => v.Value)
                    .ToList();

                    PreviewData.Add(new MaterialsConsumptionByProduct
                    {
                        RootProduct = rootProduct,
                        MaterialsComsump = materials
                    });
                }
            }
        }

        private Dictionary<string, decimal> CalcProductBomTotalQuantity(IEnumerable<ProductBomOutput> productBom)
        {
            var level1 = productBom.Where(x => x.Level == 1).ToArray();
            var productMap = new Dictionary<string, decimal>();
            foreach (var bom in level1)
            {
                var totalQuantity = bom.TotalQuantity; // không có tiêu hao, edit: Có hao hụt
                var productInfo = _existedProducts.Values.FirstOrDefault(x => x.ProductId == bom.ChildProductId);
                if (productMap.ContainsKey(productInfo.ProductCode))
                    productMap[productInfo.ProductCode] += totalQuantity;
                else
                    productMap.Add(productInfo.ProductCode, totalQuantity);

                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);

                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }

            return productMap;
        }

        private void LoopCalcProductBom(IEnumerable<ProductBomOutput> productBom, Dictionary<string, decimal> productMap, IEnumerable<ProductBomOutput> level, decimal scale)
        {
            foreach (var bom in level)
            {
                var totalQuantity = bom.Quantity * bom.Wastage * scale;
                var productInfo = _existedProducts.Values.FirstOrDefault(x => x.ProductId == bom.ChildProductId);

                if (productMap.ContainsKey(productInfo.ProductCode))
                    productMap[productInfo.ProductCode] += totalQuantity;
                else
                    productMap.Add(productInfo.ProductCode, totalQuantity);

                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);
                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }
        }

        private IList<ProductMaterialsConsumptionPreview> LoopGetMaterialConsumInherit(List<ProductMaterialsConsumptionPreview> materialsConsumptionInheri
            , IEnumerable<ProductBomOutput> productBom
            , IEnumerable<ProductBomOutput> boms
            , Dictionary<string, decimal> productMap)
        {
            var result = new List<ProductMaterialsConsumptionPreview>();
            foreach (var bom in boms)
            {
                var bomProductInfo = _existedProducts.Values.FirstOrDefault(x => x.ProductId == bom.ChildProductId);

                var hasImport = _importData.Any(x => x.UsageProductCode == bomProductInfo.ProductCode);

                var materials = hasImport ? _importData.Where(x => x.UsageProductCode == bomProductInfo.ProductCode)
                    .Select(x => new ProductMaterialsConsumptionPreview
                    {
                        BomQuantity = 1,
                        DepartmentName = x.DepartmentName ?? x.DepartmentCode,
                        GroupTitle = x.GroupTitle,
                        Quantity = x.Quantity,
                        Wastage = x.Wastage ?? 1,
                        StepName = x.StepName,
                        ProductExtraInfo = _existedProducts[x.UsageProductCode.NormalizeAsInternalName()],
                        ProductMaterialsComsumptionExtraInfo = _existedProducts[x.ProductCode.NormalizeAsInternalName()],
                        Description = x.Description,
                        IsImported = true
                    })
                    .ToList()
                    : materialsConsumptionInheri.Where(x => x.ProductExtraInfo.ProductCode == bomProductInfo.ProductCode).ToList();
                var childBom = productBom.Where(x => x.ProductId == bom.ChildProductId).ToList();
                var materialsInheri = LoopGetMaterialConsumInherit(materialsConsumptionInheri, productBom, childBom, productMap);

                var exceptMaterials = materialsInheri.Except(materials, new ProductMaterialsConsumptionPreviewComparer())
                    .Select(x => new ProductMaterialsConsumptionPreview
                    {
                        GroupTitle = x.GroupTitle,
                        ProductMaterialsComsumptionExtraInfo = x.ProductMaterialsComsumptionExtraInfo,
                        ProductExtraInfo = bomProductInfo,
                        DepartmentName = x.DepartmentName,
                        StepName = x.StepName,
                        Description = x.Description,
                        IsImported = x.IsImported
                    });

                materials.AddRange(exceptMaterials);
                foreach (var m in materials)
                {
                    m.MaterialsConsumptionInherit = materialsInheri.Where(x => x.GroupTitle == m.GroupTitle
                                    && x.ProductMaterialsComsumptionExtraInfo.ProductCode == m.ProductMaterialsComsumptionExtraInfo.ProductCode).ToList();

                    m.BomQuantity = productMap.ContainsKey(m.ProductExtraInfo.ProductCode) ? productMap[m.ProductExtraInfo.ProductCode] : 1;
                    m.TotalQuantityInheritance = m.MaterialsConsumptionInherit.Select(x => (x.Quantity * x.Wastage * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
                }

                result.AddRange(materials);
            }

            return result;
        }

        private IList<ProductMaterialsConsumptionPreview> CalcMaterialsConsumptionPreview(IEnumerable<ProductMaterialsConsumptionOutput> currentMaterialsConsumption)
        {
            var materials = new List<ProductMaterialsConsumptionPreview>();

            if (currentMaterialsConsumption == null)
                return materials;

            foreach (var m in currentMaterialsConsumption)
            {
                var material = _existedProducts.Values.FirstOrDefault(x => x.ProductId == m.MaterialsConsumptionId);
                var product = _existedProducts.Values.FirstOrDefault(x => x.ProductId == m.ProductId);
                var step = _steps.Values.FirstOrDefault(x => x.StepId == m.StepId);
                var department = _departments.FirstOrDefault(x => x.DepartmentId == m.DepartmentId);
                var group = _groupConsumptions.Values.FirstOrDefault(x => x.ProductMaterialsConsumptionGroupId == m.ProductMaterialsConsumptionGroupId);

                var item = new ProductMaterialsConsumptionPreview
                {
                    BomQuantity = m.BomQuantity,
                    Quantity = m.Quantity,
                    Wastage = m.Wastage,
                    StepName = step?.StepName,
                    DepartmentName = department?.DepartmentName,
                    GroupTitle = group?.Title,
                    ProductExtraInfo = product,
                    ProductMaterialsComsumptionExtraInfo = material,
                    Description = m.Description
                };

                if (_importData.Any(x => x.UsageProductCode == product.ProductCode && material.ProductCode == x.ProductCode))
                {
                    var row = _importData.FirstOrDefault(x => x.UsageProductCode == product.ProductCode && material.ProductCode == x.ProductCode);
                    item = new ProductMaterialsConsumptionPreview
                    {
                        BomQuantity = m.BomQuantity,
                        Quantity = row.Quantity,
                        Wastage = row.Wastage ?? 1,
                        StepName = row.StepName,
                        DepartmentName = row.DepartmentName ?? row.DepartmentCode,
                        GroupTitle = row.GroupTitle,
                        ProductExtraInfo = _existedProducts[row.UsageProductCode.NormalizeAsInternalName()],
                        ProductMaterialsComsumptionExtraInfo = _existedProducts[row.ProductCode.NormalizeAsInternalName()],
                        Description = row.Description
                    };
                }

                item.MaterialsConsumptionInherit = CalcMaterialsConsumptionPreview(m.MaterialsConsumptionInheri);
                item.TotalQuantityInheritance = item.MaterialsConsumptionInherit.Select(x => (x.Quantity * x.BomQuantity) + x.TotalQuantityInheritance).Sum();

                materials.Add(item);
            }

            return materials;
        }

        private int[] FoundRelationshipBom(IDictionary<int, IList<ProductBomOutput>> allProductBoms)
        {
            var allUsageProductCode = _importData.Select(x => x.UsageProductCode).Distinct();
            var productIds = _existedProducts.Values.Where(x => allUsageProductCode.Contains(x.ProductCode)).Select(x => x.ProductId).Distinct().ToArray();

            var childProductInBom = allProductBoms.Values.SelectMany(x => x).Select(x => x.ChildProductId).Distinct().ToList();

            return productIds.Where(x => !childProductInBom.Contains(x) && x > 0).ToArray();
        }


        private async Task RemoveOldMaterialConsumption(IEnumerable<int> productIds, int groupMaterialConsumptionId)
        {
            /* Loại bỏ row data cũ */
            var oldMaterialConsumptions = await _stockDbContext.ProductMaterialsConsumption
                .Where(x => productIds.Contains(x.ProductId) && x.ProductMaterialsConsumptionGroupId == groupMaterialConsumptionId)
                .ToListAsync();

            oldMaterialConsumptions.ForEach(x => x.IsDeleted = true);

            await _stockDbContext.SaveChangesAsync();
        }

        private async Task AddMissingUnit()
        {
            _units = (await _unitService.GetList(string.Empty, null, 1, -1, null)).List.GroupBy(u => u.UnitName.NormalizeAsInternalName())
                      .ToDictionary(u => u.Key, u => u.FirstOrDefault());


            var importedUnits = _importData.SelectMany(p => new[] { p.UnitName, p.UsageUnitName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());


            var newUnits = importedUnits.Where(t => !_units.ContainsKey(t.Key))
                .Select(t => new UnitInput()
                {
                    UnitName = t.Value,
                    UnitStatusId = EnumUnitStatus.Using
                }).ToList();
            foreach (var unit in newUnits)
            {
                var unitId = 0;

                if (!IsPreview)
                    unitId = await _unitService.AddUnit(unit);

                _units.Add(unit.UnitName.NormalizeAsInternalName(), new UnitOutput() { UnitId = unitId, UnitName = unit.UnitName, UnitStatusId = unit.UnitStatusId });
            }

        }

        private async Task AddMissingProduct()
        {
            _existedProducts = (from p in (await _stockDbContext.Product.AsNoTracking().Select(p => new SimpleProduct { ProductId = p.ProductId, ProductCode = p.ProductCode, ProductName = p.ProductName, UnitId = p.UnitId }).ToListAsync())
                                join u in _units.Values on p.UnitId equals u.UnitId into gu
                                from u in gu.DefaultIfEmpty()
                                select new SimpleProduct { ProductId = p.ProductId, ProductCode = p.ProductCode, ProductName = p.ProductName, UnitId = p.UnitId, UnitName = u?.UnitName })
           .GroupBy(p => p.ProductCode.NormalizeAsInternalName())
           .ToDictionary(p => p.Key, p => p.FirstOrDefault());
            var ctxs = await ValidateProductMaterials();
            var importProducts = _importData.SelectMany(p => new[]{
                    new {
                        p.ProductCode,
                        p.ProductName,
                        p.UnitName,
                        p.Specification,
                        p.ProductCateName,
                        p.ProductTypeCode
                    },
                    new {
                        ProductCode = p.UsageProductCode,
                        ProductName = p.UsageProductName,
                        UnitName = p.UsageUnitName,
                        Specification = p.UsageSpecification,
                        ProductCateName = p.UsageProductCateName,
                        ProductTypeCode = p.UsageProductTypeCode
                    }
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.ProductCode))
                    .Distinct()
                    .ToList()
                    .GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                    .ToDictionary(p => p.Key, p => new
                    {

                        ProductCode = p.First().ProductCode,
                        ProductName = p.First().ProductName,
                        ProductTypeCode = p.First().ProductTypeCode,
                        ProductCateName = p.First().ProductCateName,
                        UnitName = p.First().UnitName,
                        Specification = p.First().Specification,
                    });

           
            var newProducts = importProducts.Where(p => !_existedProducts.ContainsKey(p.Key))
                .Select(p =>
                {
                    ProductType type = null;

                    var productTitle = $"{p.Value.ProductCode} {p.Value.ProductName}";
                    if (string.IsNullOrWhiteSpace(p.Value.ProductTypeCode.NormalizeAsInternalName()))
                    {
                        type = _productTypes.FirstOrDefault(c => c.Value.IsDefault).Value;
                    }
                    else
                    {
                        _productTypes.TryGetValue(p.Value.ProductTypeCode.NormalizeAsInternalName(), out type);

                        if (type == null)
                        {
                            throw ImportProductTypeOfProductNotFound.BadRequestFormat(p.Value.ProductTypeCode, productTitle);
                        }
                    }


                    ProductCate cate = null;
                    if (string.IsNullOrWhiteSpace(p.Value.ProductCateName.NormalizeAsInternalName()))
                    {
                        cate = _productCates.FirstOrDefault(c => c.Value.IsDefault).Value;
                        if (cate == null)
                        {
                            throw ImportProductCateDefaultOfProductNotFound.BadRequestFormat(productTitle);

                        }
                    }
                    else
                    {
                        _productCates.TryGetValue(p.Value.ProductCateName.NormalizeAsInternalName(), out cate);

                        if (cate == null)
                        {
                            throw ImportProductCateOfProductNotFound.BadRequestFormat(p.Value.ProductCateName, productTitle);
                        }
                    }

                    _units.TryGetValue(p.Value.UnitName.NormalizeAsInternalName(), out var unit);
                    if (unit == null)
                    {
                        throw UnitOfProductNotFound.BadRequestFormat(productTitle);
                    }

                    return new ProductModel
                    {

                        ProductCode = p.Value.ProductCode,
                        ProductName = p.Value.ProductName,
                        ProductTypeId = type?.ProductTypeId,
                        ProductCateId = cate.ProductCateId,
                        UnitId = unit.UnitId,
                        UnitName = unit.UnitName,

                        Extra = new ProductModelExtra()
                        {
                            Specification = p.Value.Specification
                        },
                        StockInfo = new ProductModelStock()
                        {
                            UnitConversions = new List<ProductModelUnitConversion>()
                        }

                    };
                })
                .ToList();
            foreach (var product in newProducts)
            {
                var productId = 0;

                if (!IsPreview)
                    productId = await _productService.AddProductToDb(product);

                var unitName = product.UnitName;
                if (product.UnitId > 0)
                    unitName = _units.Values.FirstOrDefault(x => x.UnitId == product.UnitId)?.UnitName;

                _existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new SimpleProduct { ProductId = productId, ProductCode = product.ProductCode, ProductName = product.ProductName, UnitName = unitName });
            }
            foreach (var ctx in ctxs)
            {
                await ctx.ConfirmCode();
            }
        }

        private async Task AddMissingProductType()
        {
            _productTypes = (await _stockDbContext.ProductType.AsNoTracking().ToListAsync()).GroupBy(t => t.IdentityCode.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newProductTypes = _importData.SelectMany(p => new[] { p.ProductTypeCode, p.UsageProductTypeCode }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newTypes = newProductTypes.Where(t => !_productTypes.ContainsKey(t.Key))
                .Select(t => new ProductType()
                {
                    ProductTypeName = t.Value,
                    IdentityCode = t.Value.NormalizeAsInternalName()
                }).ToList();

            if (!IsPreview)
            {
                await _stockDbContext.ProductType.AddRangeAsync(newTypes);
                await _stockDbContext.SaveChangesAsync();
            }

            foreach (var t in newTypes)
            {
                _productTypes.Add(t.IdentityCode.NormalizeAsInternalName(), t);
            }
        }

        private async Task AddMissingProductCate()
        {
            _productCates = (await _stockDbContext.ProductCate.AsNoTracking().ToListAsync()).GroupBy(t => t.ProductCateName.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newProductCates = _importData.SelectMany(p => new[] { p.ProductCateName, p.UsageProductCateName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newCates = newProductCates.Where(t => !_productCates.ContainsKey(t.Key))
                .Select(t => new ProductCate()
                {
                    ProductCateName = t.Value
                }).ToList();

            if (!IsPreview)
            {
                await _stockDbContext.ProductCate.AddRangeAsync(newCates);
                await _stockDbContext.SaveChangesAsync();
            }

            foreach (var t in newCates)
            {
                _productCates.Add(t.ProductCateName.NormalizeAsInternalName(), t);
            }
        }

        private async Task AddMissingMaterialConsumptionGroup()
        {
            _groupConsumptions = (await _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().ToListAsync())
            .GroupBy(t => t.Title.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var importGroups = _importData.SelectMany(p => new[] { p.GroupTitle }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newGroups = importGroups.Where(t => !_groupConsumptions.ContainsKey(t.Key))
                .Select(t => new ProductMaterialsConsumptionGroup()
                {
                    Title = t.Value,
                    ProductMaterialsConsumptionGroupCode = t.Value.NormalizeAsInternalName()
                }).ToList();

            if (newGroups.Count > 0)
                throw ImportConsumptionGroupNotFound.BadRequestFormat(string.Join(", ", newGroups.Select(x => x.Title)));
            // if (!IsPreview)
            // {
            //     await _stockDbContext.ProductMaterialsConsumptionGroup.AddRangeAsync(newGroups);
            //     await _stockDbContext.SaveChangesAsync();
            // }

            // foreach (var t in newGroups)
            // {
            //     _groupConsumptions.Add(t.Title.NormalizeAsInternalName(), t);
            // }
        }

        private StepSimpleInfo GetStep(string StepName)
        {
            StepSimpleInfo step = null;
            if (!string.IsNullOrWhiteSpace(StepName))
                step = _steps[StepName.NormalizeAsInternalName()];

            return step;
        }
        private DepartmentSimpleModel GetDepartment(string departmentCode, string departmentName)
        {
            return _departments.FirstOrDefault(x => x.DepartmentCode == departmentCode || x.DepartmentName == departmentName);
        }

    }
}
