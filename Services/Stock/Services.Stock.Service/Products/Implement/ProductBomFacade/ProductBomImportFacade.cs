﻿using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.Product;
using Verp.Resources.Stock.Product.ProductImport;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Bom;
using static Verp.Resources.Stock.Product.ProductValidationMessage;
using static VErp.Commons.GlobalObject.InternalDataInterface.Stock.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement.ProductBomFacade
{
    public class ProductBomImportFacade
    {
        private StockDBContext _stockDbContext;
        private IUnitService _unitService;
        private IProductService _productService;
        //private IActivityLogService _activityLogService;
        private IProductBomService _productBomService;

        private IList<ProductBomImportModel> _importData;
        private IDictionary<string, ProductType> _productTypes;
        private IDictionary<string, ProductCate> _productCates;
        private IDictionary<string, UnitOutput[]> _units;
        private IDictionary<string, SimpleProduct> _existedProducts;
        private IList<StepSimpleInfo> _steps;

        private IDictionary<string, bool> _productCodeMaterials;
        private IDictionary<string, bool> _productIgnoreStep;
        private IDictionary<string, HashSet<int>> _productCodeProperties;

        private IDictionary<string, List<ProductBomImportModel>> _bomByProductCodes;

        private IManufacturingHelperService _manufacturingHelperService;
        private ObjectActivityLogFacade _productActivityLog;

        private bool IsPreview = false;
        public IList<ProductBomByProduct> PreviewData { get; private set; }

        public ProductBomImportFacade(bool isPreview)
        {
            IsPreview = isPreview;
        }
        public ProductBomImportFacade SetService(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
            return this;
        }

        public ProductBomImportFacade SetService(IUnitService unitService)
        {
            _unitService = unitService;
            return this;
        }

        public ProductBomImportFacade SetService(IProductService productService)
        {
            _productService = productService;
            return this;
        }

        public ProductBomImportFacade SetService(ObjectActivityLogFacade productActivityLog)
        {
            //_activityLogService = activityLogService;
            _productActivityLog = productActivityLog;
            return this;
        }

        public ProductBomImportFacade SetService(IProductBomService productBomService)
        {
            _productBomService = productBomService;
            return this;
        }

        public ProductBomImportFacade SetService(IManufacturingHelperService manufacturingHelperService)
        {
            _manufacturingHelperService = manufacturingHelperService;
            return this;
        }


        private int _id = -1000;
        public int GetNewId()
        {
            return _id--;
        }

        private ImportExcelMapping _mapping = null;
        public async Task<bool> ProcessData(ILongTaskResourceLockService longTaskResourceLockService, ImportExcelMapping mapping, Stream stream)
        {
            _mapping = mapping;

            _steps = await _manufacturingHelperService.GetSteps();

            using (var longTask = await longTaskResourceLockService.Accquire($"Nhập BOM mặt hàng từ excel"))
            {
                longTask.SetCurrentStep("Đọc dữ liệu từ tệp excel");

                ReadExcelData(mapping, stream);


                longTask.SetCurrentStep("Xử lý dữ liệu");
                longTask.SetTotalRows(_importData.Count);

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

                        longTask.SetCurrentStep("Thêm BOM");
                        await ImportBom(longTask);
                        if (!IsPreview)
                        {
                            await trans.CommitAsync();
                            await logBath.CommitAsync();
                        }
                    }
                }
                return true;
            }
        }


        private void ReadExcelData(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            _importData = reader.ReadSheetEntity<ProductBomImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (propertyName == nameof(ProductBomImportModel.ProductCode))
                {
                    entity.ProductCode = value;
                    return true;
                }
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductBomImportModel.IsIgnoreStep):
                            entity.IsIgnoreStep = value.IsRangeOfAllowValueForBooleanTrueValue();
                        return true;
                    case nameof(ProductBomImportModel.IsMaterial):
                        if (value.IsRangeOfAllowValueForBooleanTrueValue())
                        {
                            entity.IsMaterial = true;
                        }
                        return true;
                    case nameof(ProductBomImportModel.Wastage):
                        decimal.TryParse(value, out var v);
                        if (v > 0)
                        {
                            entity.Wastage = v;
                        }
                        else
                        {
                            entity.Wastage = 1;
                        }
                        return true;
                    case nameof(ProductBomImportModel.OutputStepName):
                        if (!string.IsNullOrEmpty(value) && !(TryGetStepId(value, out int? inputStepId) && inputStepId.HasValue))
                        {
                            throw ProductBomImportValidationMessage.StepNotFound.BadRequestFormat(value);
                        }
                        entity.OutputStepName = value;
                        return true;
                    case nameof(ProductBomImportModel.InputStepName):
                        if (!string.IsNullOrEmpty(value) && !(TryGetStepId(value, out int? outputStepId) && outputStepId.HasValue))
                        {
                            throw ProductBomImportValidationMessage.StepNotFound.BadRequestFormat(value);
                        }
                        entity.InputStepName = value;
                        return true;
                }


                if (propertyName.StartsWith(nameof(ProductBomImportModel.Properties)))
                {
                    var propertyId = int.Parse(propertyName.Substring(nameof(ProductBomImportModel.Properties).Length));
                    if (entity.Properties == null)
                    {
                        entity.Properties = new HashSet<int>();
                    }

                    if (!entity.Properties.Contains(propertyId) && value.IsRangeOfAllowValueForBooleanTrueValue())
                    {
                        entity.Properties.Add(propertyId);
                    }
                    return true;
                }


                return false;
            });
        }



        private void FindMaterial(SimpleProduct rootProduct, string parentProductCode, IList<int> paths, IList<string> pathCodes, ProductBomUpdateInfoModel model)
        {
            _existedProducts.TryGetValue(parentProductCode, out var parentInfo);

            if (_productCodeProperties.TryGetValue(parentProductCode, out var props) && rootProduct.ProductId != parentInfo.ProductId)
            {
                
                foreach (var propertyId in props)
                {
                    var propData = new ProductPropertyModel()
                    {
                        RootProductId = rootProduct.ProductId,
                        ProductId = parentInfo.ProductId,
                        PropertyId = propertyId,
                        PathProductIds = paths.ToArray(),
                        PathProductCodes = pathCodes.ToArray()
                    };
                    model.PropertiesInfo.BomProperties.Add(propData);
                }
            }

            if (_productIgnoreStep.TryGetValue(parentProductCode, out var isIgnoreProcess) && isIgnoreProcess)
            {
                model.IgnoreStepInfo.BomIgnoreSteps.Add(new ProductIgnoreStepModel()
                {
                    RootProductId = rootProduct.ProductId,
                    ProductId = parentInfo.ProductId,
                    PathProductIds = paths.ToArray(),
                    PathProductCodes = pathCodes.ToArray()
                });
            }

            if (_productCodeMaterials.TryGetValue(parentProductCode, out var isMaterial) && isMaterial)
            {
                model.MaterialsInfo.BomMaterials.Add(new ProductMaterialModel()
                {
                    RootProductId = rootProduct.ProductId,
                    ProductId = parentInfo.ProductId,
                    PathProductIds = paths.ToArray(),
                    PathProductCodes = pathCodes.ToArray()
                });
            }
            else
            {

                if (_bomByProductCodes.ContainsKey(parentProductCode) && _bomByProductCodes[parentProductCode].Count > 0)
                {
                    foreach (var b in _bomByProductCodes[parentProductCode])
                    {
                        var _paths = paths.ToList();//clone list
                        var _pathCodes = pathCodes.ToList();//clone list

                        _paths.Add(parentInfo.ProductId);
                        _pathCodes.Add(parentInfo.ProductCode);
                        _existedProducts.TryGetValue(b.ChildProductCode.NormalizeAsInternalName(), out var childInfo);
                        if (!_paths.Contains(childInfo.ProductId))
                            FindMaterial(rootProduct, childInfo.ProductCode.NormalizeAsInternalName(), _paths, _pathCodes, model);
                    }
                }
            }
        }

        private async Task ImportBom(LongTaskResourceLock longTask)
        {
            _bomByProductCodes = _importData.GroupBy(b => b.ProductCode.NormalizeAsInternalName()).ToDictionary(b => b.Key, b => b.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).Select(g => g.First()).ToList());

            _productCodeMaterials = _importData.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.Max(m => m.IsMaterial));

            _productCodeProperties = _importData.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.SelectMany(m => m.Properties ?? new HashSet<int>()).Distinct().ToHashSet());

            _productIgnoreStep = _importData.GroupBy(c=> c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c=> c.Key, c=> c.Max(m=> m.IsIgnoreStep));

            var bomData = new List<ProductBomInput>();

            foreach (var bom in _bomByProductCodes)
            {

                _existedProducts.TryGetValue(bom.Key, out var rootProductInfo);

                var productBoms = bom.Value.Select(b =>
                {
                    _existedProducts.TryGetValue(b.ChildProductCode.NormalizeAsInternalName(), out var childProduct);

                    TryGetStepId(b.InputStepName, out int? inputStepId);
                    TryGetStepId(b.OutputStepName, out int? outputStepId);

                    if (rootProductInfo.ProductId == childProduct.ProductId)
                    {
                        throw ($"Chi tiết của mặt hàng không được là chính nó {b.ProductCode} dòng {b.RowNumber}").BadRequest();
                    }

                    return new ProductBomInput()
                    {
                        ProductBomId = null,
                        ProductId = rootProductInfo.ProductId,
                        ChildProductId = childProduct.ProductId,
                        Quantity = b.Quantity,
                        Wastage = b.Wastage ?? 1,
                        InputStepId = inputStepId,
                        OutputStepId = b.IsMaterial ? null : outputStepId,
                        Description = b.Description,
                    };
                }).ToList();

                var productMaterials = new List<ProductMaterialModel>();

                var productProperties = new List<ProductPropertyModel>();
                
                var updateModel = new ProductBomUpdateInfoModel()
                {
                    BomInfo = new ProductBomUpdateInfo(productBoms),
                    MaterialsInfo = new ProductBomMaterialUpdateInfo(new List<ProductMaterialModel>(), false),
                    PropertiesInfo = new ProductBomPropertyUpdateInfo(new List<ProductPropertyModel>(), false),
                    IgnoreStepInfo = new ProductBomIgnoreStepUpdateInfo(new List<ProductIgnoreStepModel>(), true)
                };

                FindMaterial(rootProductInfo, bom.Key, new List<int>(), new List<string>(), updateModel);


                if (!IsPreview)
                {

                    await _productBomService.UpdateProductBomDb(rootProductInfo.ProductId, updateModel);

                    await _productActivityLog.LogBuilder(() => ProductActivityLogMessage.ImportBom)
                        .MessageResourceFormatDatas(rootProductInfo.ProductCode)
                        .ObjectId(rootProductInfo.ProductId)
                        .JsonData(new { _mapping, productBoms, productMaterials })
                        .CreateLog();
                }
                else
                {
                    bomData.AddRange(productBoms);
                }

                longTask.IncProcessedRows();
            }

            if (IsPreview)
            {
                LoadPreviewData(bomData);
            }
        }
   
        private void LoadPreviewData(IList<ProductBomInput> boms)
        {
            var rootProductIds = boms.Select(b => b.ProductId)
                .Distinct()
                .Where(pId => !boms.Any(b => b.ChildProductId == pId))
                .ToList();

            PreviewData = new List<ProductBomByProduct>();
            foreach (var rootProductId in rootProductIds)
            {
                var bomInfo = new ProductBomByProduct();
                var rootInfo = _existedProducts.Values.FirstOrDefault(v => v.ProductId == rootProductId);

                bomInfo.Info = new ProductRootBomInfo()
                {
                    ProductId = rootInfo.ProductId,
                    ProductCode = rootInfo.ProductCode,
                    ProductName = rootInfo.ProductName,
                    Specification = rootInfo.Specification,
                    UnitName = rootInfo.UnitName,
                    ProductCateName = _productCates.Values.FirstOrDefault(c => c.ProductCateId == rootInfo.ProductCateId)?.ProductCateName,
                    ProductTypeName = _productTypes.Values.FirstOrDefault(c => c.ProductTypeId == rootInfo.ProductTypeId)?.ProductTypeName,
                };

                bomInfo.Boms = new List<ProductBomPreviewOutput>();

                GetBoms(rootInfo, rootInfo.ProductId, 1, 1, "", new List<int>() { rootProductId }, bomInfo.Boms, boms);

                PreviewData.Add(bomInfo);
            }
        }

        private void GetBoms(SimpleProduct rootInfo, int productId, decimal quantity, int level, string numberOrder, IList<int> pathProductIds, IList<ProductBomPreviewOutput> lst, IList<ProductBomInput> boms)
        {
            var productBoms = boms.Where(b => b.ProductId == productId).ToList();
            var bomIndex = 1;
            foreach (var b in productBoms)
            {
                if (pathProductIds.Contains(b.ChildProductId)) continue;

                var childInfo = _existedProducts.Values.FirstOrDefault(v => v.ProductId == b.ChildProductId);

                var totalQuantity = quantity * (b.Quantity ?? 0) * (b.Wastage ?? 1);

                var bomNumOrder = numberOrder;

                bomNumOrder += "." + bomIndex++;
                bomNumOrder = bomNumOrder.Trim('.');


                var productCodeNormalized = childInfo?.ProductCode?.NormalizeAsInternalName();

                _productCodeProperties.TryGetValue(productCodeNormalized, out var propertyIds);

                _productCodeMaterials.TryGetValue(productCodeNormalized, out var isMaterial);

                lst.Add(new ProductBomPreviewOutput()
                {
                    ProductBomId = 0,
                    Level = level,
                    ProductId = productId,
                    ChildProductId = b.ChildProductId,

                    ProductCode = childInfo.ProductCode,

                    ProductName = childInfo.ProductName,
                    Specification = childInfo.Specification,

                    Quantity = b.Quantity ?? 0,
                    Wastage = b.Wastage ?? 1,
                    TotalQuantity = totalQuantity,
                    Description = b.Description,
                    UnitName = childInfo.UnitName,
                    UnitId = childInfo.UnitId,
                    IsMaterial = isMaterial,
                    NumberOrder = bomNumOrder,
                    ProductUnitConversionId = 0,
                    DecimalPlace = 12,
                    InputStepId = b.InputStepId,
                    OutputStepId = b.OutputStepId,
                    PathProductIds = pathProductIds?.ToArray(),
                    PropertyIds = propertyIds?.ToList()
                });

                var pathProducts = pathProductIds.DeepClone();
                pathProducts.Add(b.ChildProductId);
                GetBoms(rootInfo, b.ChildProductId, totalQuantity, level + 1, bomNumOrder, pathProducts, lst, boms);
            }
        }


        private async Task AddMissingProductType()
        {
            _productTypes = (await _stockDbContext.ProductType.AsNoTracking().ToListAsync()).GroupBy(t => t.IdentityCode.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newProductTypes = _importData.SelectMany(p => new[] { p.ProductTypeCode, p.ChildProductTypeCode }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newTypes = newProductTypes.Where(t => !_productTypes.ContainsKey(t.Key))
                .Select(t => new ProductType()
                {
                    ProductTypeId = IsPreview ? GetNewId() : 0,
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

            var newProductCates = _importData.SelectMany(p => new[] { p.ProductCateName, p.ChildProductCateName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newCates = newProductCates.Where(t => !_productCates.ContainsKey(t.Key))
                .Select(t => new ProductCate()
                {
                    ProductCateId = IsPreview ? GetNewId() : 0,
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

        private async Task AddMissingUnit()
        {
            _units = (await _unitService.GetList(string.Empty, null, 1, -1, null)).List.GroupBy(u => u.UnitName.NormalizeAsInternalName())
                      .ToDictionary(u => u.Key, u => u.ToArray());


            var importedUnits = _importData.SelectMany(p => new[] { p.UnitName, p.ChildUnitName }).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                .GroupBy(t => t.NormalizeAsInternalName())
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());


            var newUnits = importedUnits.Where(t => !_units.ContainsKey(t.Key))
                .Select(t => new UnitInput()
                {
                    UnitName = t.Value,
                    UnitStatusId = EnumUnitStatus.Using
                }).ToList();
            foreach (var uni in newUnits)
            {
                var unitId = GetNewId();
                if (!IsPreview)
                {
                    unitId = await _unitService.AddUnit(uni);
                }

                _units.Add(uni.UnitName.NormalizeAsInternalName(), new UnitOutput[] {
                    new UnitOutput { UnitId = unitId, UnitName = uni.UnitName, UnitStatusId = uni.UnitStatusId }
                });
            }

        }

        private  void ValidateProducts()
        {
            var importProducts = new List<ProductBomImportModel>();
            foreach (var productImport in _importData)
            {
                var productCodes = new List<string>(); 
                switch (_mapping.HandleFilterOptionId)
                {
                    case EnumHandleFilterOption.Default:
                        if (productImport.ProductCode == null)
                        {
                            throw new BadRequestException("Vui lòng nhập mã mặt hàng!");
                        }
                        importProducts.Add(productImport);
                        break;
                    case EnumHandleFilterOption.FitlerByNameAndSpecification:
                         productCodes = _existedProducts.Where(x => x.Value.ProductName == productImport.ProductName && x.Value.Specification == productImport.Specification).Select(x=> x.Value.ProductCode).ToList();
                        if (productCodes.Count > 1)
                        {
                            throw new BadRequestException($"Có nhiều mặt hàng giống tên: {productImport.ProductName} và quy cách: {productImport.Specification}");
                        }
                        if (productCodes.Count == 0 && string.IsNullOrEmpty( productImport.ProductCode))
                        {
                            throw new BadRequestException($"Không tìm thấy mặt hàng có tên {productImport.ProductName} và quy cách {productImport.Specification}! Vui lòng nhập mã mặt hàng để tạo mặt hàng mới!");
                        }
                        if (!string.IsNullOrEmpty(productCodes.FirstOrDefault()))
                        {
                            productImport.ProductCode = productCodes.FirstOrDefault();
                        }
                        importProducts.Add(productImport);
                        break;
                    case EnumHandleFilterOption.FilterByName:
                        productCodes = _existedProducts.Where(x => x.Value.ProductName == productImport.ProductName).Select(x => x.Value.ProductCode).ToList();
                        if (productCodes.Count > 1)
                        {
                            throw new BadRequestException($"Có nhiều mặt hàng giống tên: {productImport.ProductName}");
                        }
                        if (productCodes.Count == 0 && string.IsNullOrEmpty(productImport.ProductCode))
                        {
                            throw new BadRequestException($"Không tìm thấy mặt hàng có tên {productImport.ProductName}! Vui lòng nhập mã mặt hàng để tạo mặt hàng mới!");
                        }
                        if (!string.IsNullOrEmpty(productCodes.FirstOrDefault()))
                        {
                            productImport.ProductCode = productCodes.FirstOrDefault();
                        }
                        importProducts.Add(productImport);
                        break;
                    default:
                        break;
                }
            }
            _importData = importProducts;
        }
        private async Task AddMissingProduct()
        {
            _existedProducts = (await (
                from p in _stockDbContext.Product
                join e in _stockDbContext.ProductExtraInfo on p.ProductId equals e.ProductId into es
                from e in es.DefaultIfEmpty()
                select new { p.ProductId, p.ProductCode, p.ProductName, p.UnitId, e.Specification, p.ProductCateId, p.ProductTypeId }
                ).ToListAsync())
                .Select(p => new SimpleProduct
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitId = p.UnitId,
                    UnitName = _units.Values.SelectMany(u => u).FirstOrDefault(u => u.UnitId == p.UnitId)?.UnitName,
                    Specification = p.Specification,
                    ProductTypeId = p.ProductTypeId,
                    ProductCateId = p.ProductCateId
                }).GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                .ToDictionary(p => p.Key, p => p.FirstOrDefault());
            ValidateProducts();
            var importProducts = _importData.SelectMany(p => new[]
                    {
                    new { p.ProductCode, p.ProductName, p.ProductTypeCode, p.ProductCateName, p.UnitName, p.Specification, IsProduct=  true, IsSemi =false },
                    new { ProductCode = p.ChildProductCode, ProductName = p.ChildProductName, ProductTypeCode= p.ChildProductTypeCode,ProductCateName=p.ChildProductCateName, UnitName = p.ChildUnitName, Specification=p.ChildSpecification,IsProduct=false, IsSemi=true } }
                    ).Where(p => !string.IsNullOrWhiteSpace(p.ProductCode))
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
                        IsProduct = p.Max(d => d.IsProduct),
                        IsSemi = p.Max(d => d.IsSemi)   
                    });

            

            var newProducts = importProducts.Where(p => !_existedProducts.ContainsKey(p.Key))
                .Select(p =>
                {
                        var productTitle = $"{p.Value.ProductCode} {p.Value.ProductName}";

                        ProductType type = null;

                        if (string.IsNullOrWhiteSpace(p.Value.ProductTypeCode.NormalizeAsInternalName()))
                        {
                            type = _productTypes.FirstOrDefault(c => c.Value.IsDefault).Value;
                        }
                        else
                        {
                            _productTypes.TryGetValue(p.Value.ProductTypeCode.NormalizeAsInternalName(), out type);

                            if (type == null)
                            {
                                throw ImportProductCateOfProductNotFound.BadRequestFormat(p.Value.ProductTypeCode, productTitle);
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
                        if (unit == null || unit.Length <= 0)
                        {
                            throw UnitOfProductNotFound.BadRequestFormat(productTitle);
                        }
                   

                    return new ProductModel
                    {

                        ProductCode = p.Value.ProductCode,
                        ProductName = p.Value.ProductName,

                        ProductTypeId = type?.ProductTypeId,
                        ProductCateId = cate.ProductCateId,
                        UnitId = unit.FirstOrDefault().UnitId,
                        IsProduct = p.Value.IsProduct,
                        IsProductSemi = p.Value.IsSemi,

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
                var productId = GetNewId();
                if (!IsPreview)
                {
                    productId = await _productService.AddProductToDb(product);
                }

                _existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new SimpleProduct
                {
                    ProductId = productId,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    UnitId = product.UnitId,
                    UnitName = _units.Values.SelectMany(u => u).FirstOrDefault(u => u.UnitId == product.UnitId)?.UnitName,
                    Specification = product.Extra.Specification,
                    ProductCateId = product.ProductCateId,
                    ProductTypeId = product.ProductTypeId
                });
            }

        }

        private bool TryGetStepId(string key, out int? value)
        {

            key = string.IsNullOrEmpty(key) ? "" : key.Trim().ToLower();

            if (_steps == null || !_steps.Any(x => x.StepName.ToLower().Equals(key)))
            {
                value = null;
                return false;
            }

            var step = _steps.FirstOrDefault(x => !string.IsNullOrEmpty(key) && x.StepName.ToLower().Equals(key));

            value = step.StepId;
            return true;
        }

        private class SimpleProduct
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public int UnitId { get; set; }
            public string UnitName { get; set; }
            public string Specification { get; set; }

            public int? ProductTypeId { get; set; }
            public int? ProductCateId { get; set; }
        }

        private class ProductRootCodeDescription : Dictionary<string, string>
        {
            //public string RootProductCode { get; set; }
            //  public string Description { get; set; }
        }
    }
}
