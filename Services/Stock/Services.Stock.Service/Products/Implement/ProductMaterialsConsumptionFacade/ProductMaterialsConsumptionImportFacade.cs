using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace VErp.Services.Stock.Service.Products.Implement.ProductMaterialsConsumptionFacade
{
    public class ProductMaterialsConsumptionImportFacade
    {
        private class SimpleProduct
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string UnitName { get; set; }
            public string Specification { get; set; }
        }

        private StockDBContext _stockDbContext;

        private IOrganizationHelperService _organizationHelperService;
        private IManufacturingHelperService _manufacturingHelperService;
        private IActivityLogService _activityLogService;

        private IProductBomService _productBomService;
        private IUnitService _unitService;
        private IProductService _productService;

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

        public ProductMaterialsConsumptionImportFacade(bool isPreview){
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
            _activityLogService = activityLogService;
            return this;
        }

        public async Task<bool> ProcessData(ImportExcelMapping mapping, Stream stream, int? productId)
        {
            ReadExcelData(mapping, stream);

            if (productId.HasValue)
                await ValidWithProductBOM(productId);

            await ValidExcelData();
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                using (var logBath = _activityLogService.BeginBatchLog())
                {
                    await AddMissingProductType();
                    await AddMissingProductCate();
                    await AddMissingUnit();
                    await AddMissingProduct();
                    await AddMissingMaterialConsumptionGroup();

                    if (!IsPreview)
                    {

                        await ImportProcess();

                        await trans.CommitAsync();
                        await logBath.CommitAsync();
                    }
                    else
                    {
                        var allUsageProductCode = _importData.Select(x => x.UsageProductCode).Distinct();
                        var allProductBoms = await _productBomService.GetBoms(_existedProducts.Values.Where(x => allUsageProductCode.Contains(x.ProductCode)).Select(x => x.ProductId).ToArray());

                        LoadPreviewData(allProductBoms);
                    }

                }
            }
            return true;
        }

        private async Task ValidWithProductBOM(int? productId)
        {
            _boms = await _productBomService.GetBom(productId.Value);
            _productInfo = await _productService.ProductInfo(productId.Value);

            var notExistsUsageProductInBom = _importData.Where(x => x.UsageProductCode != _productInfo.ProductCode)
                                                        .Where(x => !_boms.Any(b => b.ProductCode == x.UsageProductCode))
                                                        .Select(x => x.UsageProductCode).ToList();
            if (notExistsUsageProductInBom.Count() > 0)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Xuất hiện các chi tiết sử dụng không tồn tại trong BOM mặt hàng. Các chi tiết sử dụng: \"{string.Join(", ", notExistsUsageProductInBom)}\"");

        }

        private async Task ValidExcelData()
        {
            var hasGreatThanTwoUsageProductUsingMaterialConsumption = _importData
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode))
                .GroupBy(x => new { x.ProductCode, GroupTitle = x.GroupTitle.NormalizeAsInternalName() })
                .Where(x => x.GroupBy(y => y.UsageProductCode).Where(y => y.Count() > 1).Count() > 1)
                .Select(x => new
                {
                    productCode = x.Key.ProductCode,
                    usageProductCode = x.GroupBy(y => y.UsageProductCode).Where(y => y.Count() > 1).Select(y => y.Select(t => t.UsageProductCode))
                }).ToList();

            if (hasGreatThanTwoUsageProductUsingMaterialConsumption.Count > 0)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Xuất hiện chi tiết sử dụng có mã trùng nhau cùng sử dụng vật liệu tiêu hao trong cùng 1 nhóm. Các vật liệu tiêu hao: \"{string.Join(", ", hasGreatThanTwoUsageProductUsingMaterialConsumption.Select(x => x.productCode))}\"");

            _departments = await _organizationHelperService.GetAllDepartmentSimples();
            _steps = (await _manufacturingHelperService.GetSteps()).GroupBy(x => x.StepName.NormalizeAsInternalName())
                                                                                       .ToDictionary(k => k.Key, v => v.First());

            foreach (var row in _importData)
            {
                if (string.IsNullOrWhiteSpace(row.DepartmentName) && string.IsNullOrWhiteSpace(row.DepartmentCode))
                    continue;

                var department = _departments.FirstOrDefault(x => x.DepartmentCode == row.DepartmentCode || x.DepartmentName == row.DepartmentName);
                if (department == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy bộ phận \"{row.DepartmentCode} {row.DepartmentName}\" của mã sản phẩm \"{row.ProductCode}\" trong hệ thống");

                if (string.IsNullOrWhiteSpace(row.StepName))
                    continue;

                if (!_steps.ContainsKey(row.StepName.NormalizeAsInternalName()))
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy công đoạn \"{row.StepName}\" của mã sản phẩm \"{row.ProductCode}\" trong hệ thống");
            }
        }

        private void ReadExcelData(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            _importData = reader.ReadSheetEntity<ImportProductMaterialsConsumptionExcelMapping>(mapping, null);
        }

        private async Task ImportProcess()
        {
            var data = _importData.GroupBy(x => x.GroupTitle.NormalizeAsInternalName());

            var lsMaterialConsumption = new List<ProductMaterialsConsumption>();

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
                                                          DepartmentId = departmentId,
                                                          StepId = stepId,
                                                          ProductMaterialsConsumptionGroupId = groupMaterialConsumptionId
                                                      };

                        lsMaterialConsumption.AddRange(newMaterialConsumptions);

                    }


                }

                if (!IsPreview)
                {
                    await _stockDbContext.ProductMaterialsConsumption.AddRangeAsync(lsMaterialConsumption);
                    await _activityLogService.CreateLog(EnumObjectType.ProductMaterialsConsumption, 0, $"Import vật liệu tiêu hao cho mặt hàng", lsMaterialConsumption.JsonSerialize());

                    await _stockDbContext.SaveChangesAsync();
                }
                
            }
        }

        private void LoadPreviewData(IDictionary<int, IList<ProductBomOutput>> allProductBoms)
        {
            var allUsageProductCode = _importData.Select(x => x.UsageProductCode.NormalizeAsInternalName()).Distinct();
            foreach (var usageProductCode in allUsageProductCode)
            {
                _existedProducts.TryGetValue(usageProductCode, out var rootProduct);
                if (allProductBoms.ContainsKey(rootProduct.ProductId))
                {

                }
                else
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
                        ProductMaterialsComsumptionExtraInfo = new SimpleProduct { ProductCode = x.ProductCode, ProductName = x.ProductName, UnitName = x.UnitName, Specification = x.Specification }
                    }))
                    .SelectMany(v => v.Value)
                    .ToList();

                }
            }
        }

        private class ProductMaterialsConsumptionPreview
        {
            public string GroupTitle { get; set; }
            public decimal Quantity { get; set; }
            public string StepName { get; set; }
            public string DepartmentName { get; set; }
            public decimal TotalQuantityInheritance { get; set; } = 0;
            public decimal BomQuantity { get; set; } = 1;
            
            public SimpleProduct ProductExtraInfo {get;set;}
            public SimpleProduct ProductMaterialsComsumptionExtraInfo {get;set;}

            public IList<ProductMaterialsConsumptionPreview> MaterialsConsumptionInheri { get; set; }

        }

        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption(int productId)
        {
            var productBom = (await _productBomService.GetBom(productId)).Where(x => !x.IsMaterial);
            var productMap = CalcProductBomTotalQuantity(productBom);

            var materialsConsumptionInheri = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => productBom.Select(y => y.ChildProductId).Contains(x.ProductId))
                .Include(x => x.MaterialsConsumption)
                .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                .ToListAsync());

            int minLevel = 1;
            var materialsInheri = LoopGetMaterialConsumInheri(materialsConsumptionInheri, productBom, productBom.Where(x => x.Level == minLevel), productMap);

            var materialsConsumption = await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                            .Where(x => x.ProductId == productId)
                            .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                            .ToListAsync();

            var exceptMaterialsConsumption = materialsInheri.Except(materialsConsumption, new ProductMaterialsConsumptionBaseComparer())
                .Select(x => new ProductMaterialsConsumptionOutput
                {
                    ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                    MaterialsConsumptionId = x.MaterialsConsumptionId,
                    ProductId = productId,
                    DepartmentId = x.DepartmentId,
                    StepId = x.StepId,
                    UnitId = x.UnitId,
                    Description = x.Description
                });

            materialsConsumption.AddRange(exceptMaterialsConsumption);
            foreach (var m in materialsConsumption)
            {
                m.MaterialsConsumptionInheri = materialsInheri.Where(x => x.ProductMaterialsConsumptionGroupId == m.ProductMaterialsConsumptionGroupId
                                && x.MaterialsConsumptionId == m.MaterialsConsumptionId).ToList();
                m.BomQuantity = 1;
                m.TotalQuantityInheritance = m.MaterialsConsumptionInheri.Select(x => (x.Quantity * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
                //m.Description = string.Join(", ", m.MaterialsConsumptionInheri.Select(d => d.Description).Distinct().ToArray());                
            }

            return materialsConsumption;
        }

        private IList<ProductMaterialsConsumptionOutput> LoopGetMaterialConsumInheri(List<ProductMaterialsConsumptionOutput> materialsConsumptionInheri
            , IEnumerable<ProductBomOutput> productBom
            , IEnumerable<ProductBomOutput> boms
            , Dictionary<int?, decimal> productMap)
        {
            var result = new List<ProductMaterialsConsumptionOutput>();
            foreach (var bom in boms)
            {
                var materials = materialsConsumptionInheri.Where(x => x.ProductId == bom.ChildProductId).ToList();
                var childBom = productBom.Where(x => x.ProductId == bom.ChildProductId).ToList();
                var materialsInheri = LoopGetMaterialConsumInheri(materialsConsumptionInheri, productBom, childBom, productMap);

                var exceptMaterials = materialsInheri.Except(materials, new ProductMaterialsConsumptionBaseComparer())
                    .Select(x => new ProductMaterialsConsumptionOutput
                    {
                        ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                        MaterialsConsumptionId = x.MaterialsConsumptionId,
                        ProductId = bom.ChildProductId.GetValueOrDefault(),
                        DepartmentId = x.DepartmentId,
                        StepId = x.StepId,
                        UnitId = x.UnitId,
                        Description = x.Description
                    });

                materials.AddRange(exceptMaterials);
                foreach (var m in materials)
                {
                    m.MaterialsConsumptionInheri = materialsInheri.Where(x => x.ProductMaterialsConsumptionGroupId == m.ProductMaterialsConsumptionGroupId
                                    && x.MaterialsConsumptionId == m.MaterialsConsumptionId).ToList();
                    m.BomQuantity = productMap.ContainsKey(m.ProductId) ? productMap[m.ProductId] : 1;
                    m.TotalQuantityInheritance = m.MaterialsConsumptionInheri.Select(x => (x.Quantity * x.BomQuantity) + x.TotalQuantityInheritance).Sum();
                }

                result.AddRange(materials);
            }

            return result;
        }

        private Dictionary<int?, decimal> CalcProductBomTotalQuantity(IEnumerable<ProductBomOutput> productBom)
        {
            var level1 = productBom.Where(x => x.Level == 1).ToArray();
            var productMap = new Dictionary<int?, decimal>();
            foreach (var bom in level1)
            {
                var totalQuantity = bom.Quantity; // không có tiêu hao
                if (productMap.ContainsKey(bom.ChildProductId))
                    productMap[bom.ChildProductId] += totalQuantity;
                else
                    productMap.Add(bom.ChildProductId, totalQuantity);

                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);

                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }

            return productMap;
        }

        private void LoopCalcProductBom(IEnumerable<ProductBomOutput> productBom, Dictionary<int?, decimal> productMap, IEnumerable<ProductBomOutput> level, decimal scale)
        {
            foreach (var bom in level)
            {
                var totalQuantity = bom.Quantity * bom.Wastage * scale;
                if (productMap.ContainsKey(bom.ChildProductId))
                    productMap[bom.ChildProductId] += totalQuantity;
                else
                    productMap.Add(bom.ChildProductId, totalQuantity);
                var childs = productBom.Where(x => x.ProductId == bom.ChildProductId);
                LoopCalcProductBom(productBom, productMap, childs, totalQuantity);
            }
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
                var unitId = 0 ;

                if (!IsPreview)
                    unitId = await _unitService.AddUnit(unit);
                
                _units.Add(unit.UnitName.NormalizeAsInternalName(), new UnitOutput() { UnitId = unitId, UnitName = unit.UnitName, UnitStatusId = unit.UnitStatusId });
            }

        }

        private async Task AddMissingProduct()
        {
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

            _existedProducts = (await _stockDbContext.Product.AsNoTracking().Select(p => new SimpleProduct { ProductId = p.ProductId, ProductCode = p.ProductCode, ProductName = p.ProductName }).ToListAsync()).GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                .ToDictionary(p => p.Key, p => p.FirstOrDefault());

            var newProducts = importProducts.Where(p => !_existedProducts.ContainsKey(p.Key))
                .Select(p =>
                {
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
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy loại mã mặt hàng {p.Value.ProductTypeCode} cho mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");
                        }
                    }


                    ProductCate cate = null;
                    if (string.IsNullOrWhiteSpace(p.Value.ProductCateName.NormalizeAsInternalName()))
                    {
                        cate = _productCates.FirstOrDefault(c => c.Value.IsDefault).Value;
                        if (cate == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy danh mục mặc định cho mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");

                        }
                    }
                    else
                    {
                        _productCates.TryGetValue(p.Value.ProductCateName.NormalizeAsInternalName(), out cate);

                        if (cate == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy danh mục {p.Value.ProductCateName} mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");
                        }
                    }

                    _units.TryGetValue(p.Value.UnitName.NormalizeAsInternalName(), out var unit);
                    if (unit == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị tính \"{p.Value.UnitName}\" cho mặt hàng {p.Value.ProductCode} {p.Value.ProductName}");
                    }

                    return new ProductModel
                    {

                        ProductCode = p.Value.ProductCode,
                        ProductName = p.Value.ProductName,
                        ProductTypeId = type?.ProductTypeId,
                        ProductCateId = cate.ProductCateId,
                        UnitId = unit.UnitId,

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
                
                _existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new SimpleProduct { ProductId = productId, ProductCode = product.ProductCode, ProductName = product.ProductName });
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
                    IdentityCode = t.Value
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

            if (!IsPreview)
            {
                await _stockDbContext.ProductMaterialsConsumptionGroup.AddRangeAsync(newGroups);
                await _stockDbContext.SaveChangesAsync();
            }

            foreach (var t in newGroups)
            {
                _groupConsumptions.Add(t.Title.NormalizeAsInternalName(), t);
            }
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
