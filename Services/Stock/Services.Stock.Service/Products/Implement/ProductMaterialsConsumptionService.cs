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

        private readonly IUnitService _unitService;
        private readonly IProductService _productService;

        public ProductMaterialsConsumptionService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IUnitService unitService
            , IProductService productService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _unitService = unitService;
            _productService = productService;
        }

        public async Task<IEnumerable<ProductMaterialsConsumptionModel>> GetProductMaterialsConsumptionService(int productId)
        {
            return await _stockDbContext.ProductMaterialsConsumption.AsNoTracking()
                .Where(x => x.ProductId == productId)
                .ProjectTo<ProductMaterialsConsumptionModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> UpdateProductMaterialsConsumptionService(int productId, ICollection<ProductMaterialsConsumptionModel> model)
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

            var result = new  List<ProductMaterialsConsumptionModel>();
            foreach (var row in data)
            {
                if (!products.ContainsKey(row.ProductCode)) continue;

                var item = new ProductMaterialsConsumptionModel
                {
                    ProductMaterialsConsumptionId = products[row.ProductCode],
                    GroupCode = row.GroupCode,
                    GroupTitle = row.GroupTitle,
                    ProductId = productId,
                    Quantity = row.Quantity
                };

                result.Add(item);
            }
            return await UpdateProductMaterialsConsumptionService(productId, result);
        }

    }
}
