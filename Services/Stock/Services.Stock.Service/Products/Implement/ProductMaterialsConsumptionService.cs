using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductMaterialsConsumptionService: IProductMaterialsConsumptionService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        private readonly IProductBomService _productBomService;

        public ProductMaterialsConsumptionService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IProductBomService productBomService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _productBomService = productBomService;
        }

        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumptionService(int productId)
        {
            var productBom = (await _productBomService.GetBom(productId)).Where(x => !x.IsMaterial);

            var productMap = CalcProductBomTotalQuantity(productBom);

            var materialsConsumInheri = _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => productMap.Keys.Contains(x.ProductId))
                .ProjectTo<ProductMaterialsConsumptionInput>(_mapper.ConfigurationProvider)
                .ToList();
            materialsConsumInheri.ForEach(x => x.Quantity *= productMap[x.MaterialsConsumptionId]);

            var result =  materialsConsumInheri.GroupBy(x => x.MaterialsConsumptionId)
                .Select(g => new ProductMaterialsConsumptionOutput
                {
                    Quantity = decimal.Zero,
                    MaterialsConsumptionId = g.Key,
                    DepartmentId = g.First().DepartmentId,
                    StepId = g.First().StepId,
                    TotalQuantityInheritance = g.Sum(x => x.Quantity),
                    ProductId = productId,
                }).ToList();

            var materialsConsum = _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => x.ProductId == productId)
                .ProjectTo<ProductMaterialsConsumptionOutput>(_mapper.ConfigurationProvider)
                .ToList();

            foreach(var m in materialsConsum)
            {
                var mInheri = materialsConsumInheri.FirstOrDefault(x => x.MaterialsConsumptionId == m.MaterialsConsumptionId);
                if (mInheri == null)
                    result.Add(m);
                else
                {
                    mInheri.Quantity = m.Quantity;
                    mInheri.StepId = m.StepId;
                    mInheri.DepartmentId = m.DepartmentId;
                }
            }

            return result;
        }

        private Dictionary<int?, decimal> CalcProductBomTotalQuantity(IEnumerable<ProductBomOutput> productBom)
        {
            var level1 = productBom.Where(x => x.Level == 1).ToArray();
            var productMap = new Dictionary<int?, decimal>();
            foreach (var bom in level1)
            {
                var totalQuantity = bom.Quantity * bom.Wastage;
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

        public async Task<bool> UpdateProductMaterialsConsumptionService(int productId, ICollection<ProductMaterialsConsumptionInput> model)
        {
            var product = _stockDbContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            if (product == null) 
                throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var materials = await _stockDbContext.ProductMaterialsConsumption
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var newMaterials = model.Where(x => x.ProductMaterialsConsumptionId > 0).AsQueryable()
                .ProjectTo<ProductMaterialsConsumption>(_mapper.ConfigurationProvider).ToArray();

            foreach(var m in materials)
            {
                var s = model.FirstOrDefault(x => x.ProductMaterialsConsumptionId == m.ProductMaterialsConsumptionId);
                if (s != null)
                {
                    _mapper.Map(s, m);
                }
                else m.IsDeleted = true;
            }

            _stockDbContext.ProductMaterialsConsumption.AddRange(newMaterials);

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductMaterialsConsumption, productId, $"Cập nhật vật tư tiêu hao cho sản phẩm '{product.ProductCode}/ {product.ProductName}'", model.JsonSerialize());
            return true;
        }

        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "ProductMaterialsConsumption",
                CategoryTitle = "Định mức vật tư tiêu hao",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<ImportProductMaterialsConsumptionExcelMapping>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportMaterialsConsumptionFromMapping(int productId, ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            var data = reader.ReadSheetEntity<ImportProductMaterialsConsumptionExcelMapping>(mapping, null);

            var products = (await _stockDbContext.Product.AsNoTracking().ToListAsync()).ToDictionary(k => k.ProductCode, v => v.ProductId);
            var oldMaterialConsumption = (await _stockDbContext.ProductMaterialsConsumption.AsNoTracking().ToListAsync()).Select(k => k.MaterialsConsumptionId);

            foreach (var row in data)
            {
                if (!products.ContainsKey(row.ProductCode) || oldMaterialConsumption.Contains(products[row.ProductCode])) continue;

                var item = new ProductMaterialsConsumption
                {
                    MaterialsConsumptionId = products[row.ProductCode],
                    ProductId = productId,
                    Quantity = row.Quantity
                };

                _stockDbContext.ProductMaterialsConsumption.Add(item);
            }

            await _stockDbContext.SaveChangesAsync();
            return true;
        }

    }
}
