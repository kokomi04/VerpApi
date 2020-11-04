using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using Microsoft.Data.SqlClient;
using VErp.Infrastructure.EF.EFExtensions;
using AutoMapper;

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductBomService : IProductBomService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public ProductBomService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<ProductBomService> logger
            , IActivityLogService activityLogService
            , IMapper mapper)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<IList<ProductBomOutput>> GetBOM(int productId)
        {
            if (!_stockDbContext.Product.Any(p => p.ProductId == productId)) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var sql = @$"WITH prd_bom AS (
		                        SELECT
				                        ProductBomId,
				                        ProductId,
				                        ParentProductId,
				                        1 AS Level,
				                        Quantity,
				                        Wastage
		                        FROM ProductBom
		                        WHERE ParentProductId = @ProductId AND IsDeleted = 0
		                        UNION ALL
		                        SELECT
				                        child.ProductBomId,
				                        child.ProductId, 
				                        child.ParentProductId,
				                        bom.Level + 1 AS Level,
				                        child.Quantity,
				                        child.Wastage
		                        FROM
				                        ProductBom child
				                        INNER JOIN prd_bom bom
						                        ON bom.ProductId = child.ParentProductId
                                        WHERE child.IsDeleted = 0
                        )
                        SELECT prd_bom.*, p.ProductCode, p.ProductName, u.UnitName FROM prd_bom 
                        LEFT JOIN Product p ON prd_bom.ProductId = p.ProductId
                        LEFT JOIN ProductExtraInfo pei ON prd_bom.ProductId = pei.ProductId
                        LEFT JOIN v_Unit u ON p.UnitId = u.F_Id;";

            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productId)
            };

            var resultData = await _stockDbContext.QueryDataTable(sql, parammeters);

            return resultData.ConvertData<ProductBomOutput>();
        }

        public async Task<bool> Update(int productId, IList<ProductBomInput> req)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            // Validate data
            var productIds = req.Select(b => b.ProductId).ToList();
            var parentIds = req.Select(b => b.ParentProductId.Value).ToList();
            var allProductIds = productIds.Union(parentIds).Distinct().ToList();

            if (_stockDbContext.Product.Count(p => allProductIds.Contains(p.ProductId)) != allProductIds.Count) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            // Validate parent product id
            if (parentIds.Any(p => p != productId && !productIds.Contains(p))) throw new BadRequestException(GeneralCode.InvalidParams);

            // Validate duplicate


            // Get old BOM info
            var sql = @$"WITH prd_bom AS (
		                    SELECT
                                    ProductBomId,
				                    ProductId,
				                    ParentProductId,
				                    Quantity,
				                    Wastage
		                    FROM ProductBom
		                    WHERE ParentProductId = @ProductId AND IsDeleted = 0
		                    UNION ALL
		                    SELECT
                                    child.ProductBomId,
				                    child.ProductId, 
				                    child.ParentProductId,
				                    child.Quantity,
				                    child.Wastage
		                    FROM
				                    ProductBom child
				                    INNER JOIN prd_bom bom
						                    ON bom.ProductId = child.ParentProductId
                            WHERE child.IsDeleted = 0
                    )
                    SELECT prd_bom.* FROM prd_bom;";
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productId)
            };
            var resultData = await _stockDbContext.QueryDataTable(sql, parammeters);
            var oldBoms = resultData.ConvertData<ProductBomInput>();
            var newBoms = new List<ProductBomInput>(req);
            var changeBoms = new List<(ProductBomInput OldValue, ProductBomInput NewValue)>();

            foreach (var newItem in req)
            {
                var oldBom = oldBoms.FirstOrDefault(b => b.ProductId == newItem.ProductId && b.ParentProductId == newItem.ParentProductId);
                if (oldBom != null)
                {
                    if (HasChange(oldBom, newItem))
                    {
                        changeBoms.Add((oldBom, newItem));
                    }
                    newBoms.Remove(newItem);
                    oldBoms.Remove(oldBom);
                }
            }

            // delete old bom
            if (oldBoms.Count > 0)
            {
                var deleteBomIds = oldBoms.Select(b => b.ProductBomId).ToList();
                var deleteBoms = _stockDbContext.ProductBom.Where(b => deleteBomIds.Contains(b.ProductBomId)).ToList();
                foreach (var deleteBom in deleteBoms)
                {
                    deleteBom.IsDeleted = true;
                }
            }

            // create new bom
            if (newBoms.Count > 0)
            {
                foreach (var newBom in newBoms)
                {
                    var entity = _mapper.Map<ProductBom>(newBom);
                    _stockDbContext.ProductBom.Add(entity);
                }
            }

            // update bom
            if(changeBoms.Count > 0)
            {
                var updateBomIds = changeBoms.Select(b => b.OldValue.ProductBomId).ToList();
                var updateBoms = _stockDbContext.ProductBom.Where(b => updateBomIds.Contains(b.ProductBomId)).ToList();
                foreach (var updateBom in changeBoms)
                {
                    var entity = updateBoms.First(b => b.ProductBomId == updateBom.OldValue.ProductBomId);
                    entity.Quantity = updateBom.NewValue.Quantity;
                    entity.Wastage = updateBom.NewValue.Wastage;
                }
            }
           
            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, productId, $"Cập nhật chi tiết bom cho mặt hàng {product.ProductCode}, tên hàng {product.ProductName}", req.JsonSerialize());
            return true;
        }

        private bool HasChange(ProductBomInput oldValue, ProductBomInput newValue)
        {
            return oldValue.Quantity != newValue.Quantity
                || oldValue.Wastage != newValue.Wastage;
        }

    }
}
