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

namespace VErp.Services.Stock.Service.Products.Implement
{
    public class ProductBomService : IProductBomService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public ProductBomService(StockDBContext stockContext
           , IOptions<AppSetting> appSetting
           , ILogger<ProductBomService> logger
           , IActivityLogService activityLogService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<IList<ProductBomOutput>> GetBOM(long productBomId)
        {
            var sql = @$"WITH prd_bom AS (
                            SELECT
                                @ProductId AS ProductId,
                                NULL AS ParentProductId,
                                0 AS Level,
                                CONVERT(DECIMAL(18, 4), 1) AS Quantity,
                                CONVERT(DECIMAL(18, 4), 1) AS Wastage,
                                CONVERT(DECIMAL(18, 4), 1) AS Total
                            UNION ALL
                            SELECT
                                child.ProductId, 
                                child.ParentProductId,
				                bom.Level + 1 AS Level,
                                child.Quantity,
				                child.Wastage,
				                CONVERT(DECIMAL(18, 4), (child.Quantity * child.Wastage) * bom.Total) AS Total
                            FROM
                                ProductBom child
                                INNER JOIN prd_bom bom
                                    ON bom.ProductId = child.ParentProductId
                        )
                        SELECT prd_bom.*, p.ProductCode, p.ProductName, u.UnitName FROM prd_bom 
                        LEFT JOIN Product p ON prd_bom.ProductId = p.ProductId
                        LEFT JOIN ProductExtraInfo pei ON prd_bom.ProductId = pei.ProductId
                        LEFT JOIN v_Unit u ON p.UnitId = u.F_Id; ";

            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productBomId)
            };

            var resultData = await _stockDbContext.QueryDataTable(sql, parammeters);

            return resultData.ConvertData<ProductBomOutput>();
        }

        public async Task<long> Add(ProductBomInput req)
        {

            var checkExists = _stockDbContext.ProductBom.Any(q => q.ProductId == req.ProductId && q.ParentProductId == req.ParentProductId);
            if (checkExists)
                throw new BadRequestException(GeneralCode.InvalidParams);
            var entity = new ProductBom
            {
                //Level = 0,
                ProductId = req.ProductId,
                ParentProductId = req.ParentProductId,
                Quantity = req.Quantity,
                Wastage = req.Wastage,
                Description = req.Description,
                IsDeleted = false
            };
            await _stockDbContext.ProductBom.AddAsync(entity);
            await _stockDbContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductBomId, $"Thêm mới 1 chi tiết bom {entity.ProductId}", req.JsonSerialize());

            return entity.ProductBomId;

        }

        public async Task<bool> Update(long productBomId, ProductBomInput req)
        {

            if (productBomId <= 0)
                throw new BadRequestException(GeneralCode.InvalidParams);
            var entity = _stockDbContext.ProductBom.FirstOrDefault(q => q.ProductBomId == productBomId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            entity.Quantity = req.Quantity;
            entity.Wastage = req.Wastage;
            entity.Description = req.Description;
            entity.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductBomId, $"Cập nhật chi tiết bom {entity.ProductId} {entity.ParentProductId}", req.JsonSerialize());
            return true;

        }


        public async Task<bool> Delete(long productBomId, int rootProductId)
        {

            var entity = _stockDbContext.ProductBom.FirstOrDefault(q => q.ProductBomId == productBomId);
            if (entity == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            entity.IsDeleted = true;
            entity.UpdatedDatetimeUtc = DateTime.UtcNow;
            var childList = new List<ProductBom>();
            foreach (var item in childList)
            {
                item.IsDeleted = true;
                item.UpdatedDatetimeUtc = DateTime.UtcNow;
            }
            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, entity.ProductId, $"Xóa thông tin bom {entity.ProductId}", entity.JsonSerialize());

            return true;

        }
    }
}
