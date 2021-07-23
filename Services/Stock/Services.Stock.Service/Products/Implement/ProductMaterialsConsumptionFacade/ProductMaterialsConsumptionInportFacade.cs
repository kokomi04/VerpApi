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
   

    public class ProductMaterialsConsumptionInportFacade
    {
        private class SimpleProduct
        {
            public int ProductId { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
        }

        private readonly StockDBContext _stockDbContext;

        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;
        private readonly IActivityLogService _activityLogService;
        private IUnitService _unitService;
        private IProductService _productService;

        private IList<ImportProductMaterialsConsumptionExcelMapping> _importData;
        private IDictionary<string, UnitOutput> _units;
        private IDictionary<string, SimpleProduct> _existedProducts;
        private IDictionary<string, ProductType> _productTypes;
        private IDictionary<string, ProductCate> _productCates;
        private IList<DepartmentSimpleModel> _departments;
        private IList<StepSimpleInfo> _steps;

        public ProductMaterialsConsumptionInportFacade SetService(IUnitService unitService)
        {
            _unitService = unitService;
            return this;
        }
        public ProductMaterialsConsumptionInportFacade SetService(IProductService productService)
        {
            _productService = productService;
            return this;
        }

        public ProductMaterialsConsumptionInportFacade(StockDBContext stockDbContext
            , IOrganizationHelperService organizationHelperService
            , IManufacturingHelperService manufacturingHelperService
            , IActivityLogService activityLogService)
        {
            _stockDbContext = stockDbContext;
            _organizationHelperService = organizationHelperService;
            _manufacturingHelperService = manufacturingHelperService;
            _activityLogService = activityLogService;
        }

        public async Task<bool> ProcessData(ImportExcelMapping mapping, Stream stream, int productId, int materialsConsumptionGroupId)
        {
            ReadExcelData(mapping, stream);
            await ValiExcelData();
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                using (var logBath = _activityLogService.BeginBatchLog())
                {
                    await AddMissingProductType();
                    await AddMissingProductCate();
                    await AddMissingUnit();
                    await AddMissingProduct();
                    await Import(productId, materialsConsumptionGroupId);
                    await trans.CommitAsync();
                    await logBath.CommitAsync();
                }
            }
            return true;
        }

        private async Task ValiExcelData()
        {
            var hasTwoProduct = _importData
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode))
                .GroupBy(x => x.ProductCode)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);
            if (hasTwoProduct.Count() > 0)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Xuất hiện hơn 1 sản phẩm trở lên trong file import. Chi tiết các mã sản phẩm: \"{string.Join(", ", hasTwoProduct)}\"");

            _departments = await _organizationHelperService.GetAllDepartmentSimples();
            _steps = await _manufacturingHelperService.GetSteps();

            foreach (var row in _importData)
            {
                if (string.IsNullOrWhiteSpace(row.DepartmentName) && string.IsNullOrWhiteSpace(row.DepartmentCode))
                    continue;

                var department = _departments.FirstOrDefault(x => x.DepartmentCode == row.DepartmentCode || x.DepartmentName == row.DepartmentName);
                if (department == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy bộ phận \"{row.DepartmentCode} {row.DepartmentName}\" của mã sản phẩm \"{row.ProductCode}\" trong hệ thống");

                if (string.IsNullOrWhiteSpace(row.StepName))
                    continue;

                var step = _steps.FirstOrDefault(x => x.StepName == row.StepName);
                if (step == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy công đoạn \"{row.StepName}\" của mã sản phẩm \"{row.ProductCode}\" trong hệ thống");
            }
        }

        private void ReadExcelData(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            _importData = reader.ReadSheetEntity<ImportProductMaterialsConsumptionExcelMapping>(mapping, null);
        }

        private async Task Import(int productId, int materialsConsumptionGroupId)
        {
            var groups = (await _stockDbContext.ProductMaterialsConsumptionGroup.AsNoTracking().ToListAsync()).ToDictionary(k => k.ProductMaterialsConsumptionGroupCode, v => v.ProductMaterialsConsumptionGroupId);

            

            var oldMaterialConsumption = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => x.ProductId == productId && x.ProductMaterialsConsumptionGroupId == materialsConsumptionGroupId)
                .ToListAsync()).Select(k => k.MaterialsConsumptionId).ToList();

            foreach (var row in _importData)
            {
                if (!_existedProducts.ContainsKey(row.ProductCode.NormalizeAsInternalName()) || oldMaterialConsumption.Contains(_existedProducts[row.ProductCode.NormalizeAsInternalName()].ProductId)) continue;

                var department = _departments.FirstOrDefault(x => x.DepartmentCode == row.DepartmentCode || x.DepartmentName == row.DepartmentName);
                var step = _steps.FirstOrDefault(x => x.StepName == row.StepName);
                var item = new ProductMaterialsConsumption
                {
                    MaterialsConsumptionId = _existedProducts[row.ProductCode.NormalizeAsInternalName()].ProductId,
                    ProductId = productId,
                    Quantity = row.Quantity,
                    DepartmentId = department?.DepartmentId,
                    StepId = step?.StepId,
                    ProductMaterialsConsumptionGroupId = materialsConsumptionGroupId,
                    Description= row.Description
                };

                _stockDbContext.ProductMaterialsConsumption.Add(item);
                oldMaterialConsumption.Add(item.MaterialsConsumptionId);
            }

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Product, productId, $"Import vật liệu tiêu hao cho sản phẩm {productId} nhóm {materialsConsumptionGroupId} ", _importData.JsonSerialize());
        }

        private async Task AddMissingUnit()
        {
            _units = (await _unitService.GetList(string.Empty, null, 1, -1, null)).List.GroupBy(u => u.UnitName.NormalizeAsInternalName())
                      .ToDictionary(u => u.Key, u => u.FirstOrDefault());


            var importedUnits = _importData.SelectMany(p => new[] { p.UnitName}).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
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
            var importProducts = _importData.Select(p => new
                {
                    p.ProductCode,
                    p.ProductName,
                    p.UnitName,
                    p.Specification,
                    p.ProductCateName,
                    p.ProductTypeCode
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.ProductCode))
                    .Distinct()
                    .ToList()
                    .GroupBy(p => p.ProductCode.NormalizeAsInternalName())
                    .ToDictionary(p => p.Key, p => p.FirstOrDefault());

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
                var productId = await _productService.AddProductToDb(product);
                _existedProducts.Add(product.ProductCode.NormalizeAsInternalName(), new SimpleProduct { ProductId = productId, ProductCode = product.ProductCode, ProductName = product.ProductName });
            }
        }

        private async Task AddMissingProductType()
        {
            _productTypes = (await _stockDbContext.ProductType.AsNoTracking().ToListAsync()).GroupBy(t => t.IdentityCode.NormalizeAsInternalName()).ToDictionary(t => t.Key, t => t.FirstOrDefault());

            var newProductTypes = _importData.SelectMany(p => new[] { p.ProductTypeCode}).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
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

            var newProductCates = _importData.SelectMany(p => new[] { p.ProductCateName}).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
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

        
    }
}
