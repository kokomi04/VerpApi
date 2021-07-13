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

namespace VErp.Services.Stock.Service.Products.Implement.ProductBomFacade
{
    public class ProductBomImportFacade
    {
        private StockDBContext _stockDbContext;
        private IUnitService _unitService;
        private IProductService _productService;
        private IActivityLogService _activityLogService;
        private IProductBomService _productBomService;

        private IList<ProductBomImportModel> _importData;
        private IDictionary<string, ProductType> _productTypes;
        private IDictionary<string, ProductCate> _productCates;
        private IDictionary<string, UnitOutput> _units;
        private IDictionary<string, SimpleProduct> _existedProducts;
        private IList<StepSimpleInfo> _steps;

        private IDictionary<string, bool> _productCodeMaterials;
        private IDictionary<string, HashSet<int>> _productCodeProperties;
        private IDictionary<string, List<ProductBomImportModel>> _bomByProductCodes;

        private IManufacturingHelperService _manufacturingHelperService;

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

        public ProductBomImportFacade SetService(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
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

        public async Task<bool> ProcessData(ImportExcelMapping mapping, Stream stream)
        {
            _steps = await _manufacturingHelperService.GetSteps();

            ReadExcelData(mapping, stream);

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                using (var logBath = _activityLogService.BeginBatchLog())
                {
                    await AddMissingProductType();
                    await AddMissingProductCate();
                    await AddMissingUnit();
                    await AddMissingProduct();
                    await ImportBom();
                    await trans.CommitAsync();
                    await logBath.CommitAsync();
                }
            }
            return true;
        }


        private void ReadExcelData(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            _importData = reader.ReadSheetEntity<ProductBomImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(ProductBomImportModel.IsMaterial):
                        if (value.NormalizeAsInternalName().Equals("Có".NormalizeAsInternalName()))
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
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Công đoạn \"{value}\" không có trên hệ thống");
                        }
                        entity.OutputStepName = value;
                        return true;
                    case nameof(ProductBomImportModel.InputStepName):
                        if (!string.IsNullOrEmpty(value) && !(TryGetStepId(value, out int? outputStepId) && outputStepId.HasValue))
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Công đoạn \"{value}\" không có trên hệ thống");
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

                    if (!entity.Properties.Contains(propertyId) && value.NormalizeAsInternalName().Equals("Có".NormalizeAsInternalName()))
                    {
                        entity.Properties.Add(propertyId);
                    }
                    return true;
                }


                return false;
            });
        }


        private void FindMaterial(int rootProductId, string parentProductCode, IList<int> paths, IList<string> pathCodes, IList<ProductMaterialModel> productMaterials, List<ProductPropertyModel> productProperties)
        {
            _existedProducts.TryGetValue(parentProductCode, out var parentInfo);

            if (_productCodeProperties.TryGetValue(parentProductCode, out var props) && rootProductId != parentInfo.ProductId)
            {
                var propData = props.Select(propertyId => new ProductPropertyModel()
                {
                    RootProductId = rootProductId,
                    ProductId = parentInfo.ProductId,
                    PropertyId = propertyId,
                    PathProductIds = paths.ToArray(),
                    PathProductCodes = pathCodes.ToArray()
                });

                productProperties.AddRange(propData);
            }


            if (_productCodeMaterials.TryGetValue(parentProductCode, out var isMaterial) && isMaterial)
            {
                productMaterials.Add(new ProductMaterialModel()
                {
                    RootProductId = rootProductId,
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
                            FindMaterial(rootProductId, childInfo.ProductCode.NormalizeAsInternalName(), _paths, _pathCodes, productMaterials, productProperties);
                    }
                }
            }
        }

        private async Task ImportBom()
        {
            _bomByProductCodes = _importData.GroupBy(b => b.ProductCode.NormalizeAsInternalName()).ToDictionary(b => b.Key, b => b.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).Select(g => g.First()).ToList());

            _productCodeMaterials = _importData.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.Max(m => m.IsMaterial));

            _productCodeProperties = _importData.GroupBy(c => c.ChildProductCode.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.SelectMany(m => m.Properties ?? new HashSet<int>()).Distinct().ToHashSet());

            foreach (var bom in _bomByProductCodes)
            {
                _existedProducts.TryGetValue(bom.Key, out var productInfo);

                var productMaterials = new List<ProductMaterialModel>();

                var productProperties = new List<ProductPropertyModel>();

                FindMaterial(productInfo.ProductId, bom.Key, new List<int>(), new List<string>(), productMaterials, productProperties);


                var productBoms = bom.Value.Select(b =>
                {
                    _existedProducts.TryGetValue(b.ChildProductCode.NormalizeAsInternalName(), out var childProduct);

                    TryGetStepId(b.InputStepName, out int? inputStepId);
                    TryGetStepId(b.OutputStepName, out int? outputStepId);

                    return new ProductBomInput()
                    {
                        ProductBomId = null,
                        ProductId = productInfo.ProductId,
                        ChildProductId = childProduct.ProductId,
                        Quantity = b.Quantity,
                        Wastage = b.Wastage ?? 1,
                        InputStepId = inputStepId,
                        OutputStepId = b.IsMaterial ? null : outputStepId
                    };
                }).ToList();

                await _productBomService.UpdateProductBomDb(productInfo.ProductId, productBoms, productMaterials, productProperties, false, false);

                await _activityLogService.CreateLog(EnumObjectType.ProductBom, productInfo.ProductId, $"Cập nhật chi tiết bom cho mặt hàng {productInfo.ProductCode}, tên hàng {productInfo.ProductName} (import)", new { productBoms, productMaterials }.JsonSerialize());
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
                    ProductTypeName = t.Value,
                    IdentityCode = t.Value
                }).ToList();
            await _stockDbContext.ProductType.AddRangeAsync(newTypes);
            await _stockDbContext.SaveChangesAsync();

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
                    ProductCateName = t.Value
                }).ToList();

            await _stockDbContext.ProductCate.AddRangeAsync(newCates);
            await _stockDbContext.SaveChangesAsync();

            foreach (var t in newCates)
            {
                _productCates.Add(t.ProductCateName.NormalizeAsInternalName(), t);
            }
        }

        private async Task AddMissingUnit()
        {
            _units = (await _unitService.GetList(string.Empty, null, 1, -1, null)).List.GroupBy(u => u.UnitName.NormalizeAsInternalName())
                      .ToDictionary(u => u.Key, u => u.FirstOrDefault());


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
                var unitId = await _unitService.AddUnit(uni);
                _units.Add(uni.UnitName.NormalizeAsInternalName(), new UnitOutput() { UnitId = unitId, UnitName = uni.UnitName, UnitStatusId = uni.UnitStatusId });
            }

        }

        private async Task AddMissingProduct()
        {
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
                        IsSemi = p.Max(d => d.IsSemi),
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
                var productId = await _productService.AddProductToDb(product);
                _existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new SimpleProduct { ProductId = productId, ProductCode = product.ProductCode, ProductName = product.ProductName });
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
        }
    }
}
