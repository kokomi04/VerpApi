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
using AutoMapper.QueryableExtensions;

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

        public async Task<IList<ProductBomOutput>> GetBom(int productId)
        {
            if (!_stockDbContext.Product.Any(p => p.ProductId == productId)) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            var sql = @$"WITH prd_bom AS (
		                        SELECT
				                        ProductBomId,
				                        ProductId,
				                        ChildProductId,
				                        ProductId ParentProductId,
				                        1 AS Level,
				                        Quantity,
				                        Wastage,
                                        CONVERT(nvarchar(max), CONCAT('""', ProductId, '""')) AS BranchIds,
                                        CONVERT(nvarchar(max), ROW_NUMBER() OVER(ORDER BY ProductBomId)) AS NumberOrder
                                FROM ProductBom
		                        WHERE ProductId = @ProductId AND IsDeleted = 0
		                        UNION ALL
		                        SELECT
				                        child.ProductBomId,
				                        child.ProductId, 
				                        child.ChildProductId,
				                        bom.ProductId ParentProductId,
				                        bom.Level + 1 AS Level,
				                        child.Quantity,
				                        child.Wastage,
                                        CONVERT(nvarchar(max), CONCAT(bom.BranchIds, ',""', child.ProductId, '""')) AS BranchIds,
                                        CONVERT(nvarchar(max), CONCAT(bom.NumberOrder,'.', ROW_NUMBER() OVER(ORDER BY child.ProductBomId))) NumberOrder
                                FROM
				                        ProductBom child
				                        INNER JOIN prd_bom bom ON bom.ChildProductId = child.ProductId
				                        WHERE child.IsDeleted = 0 
                                            AND NOT EXISTS (SELECT 1 FROM ProductMaterial m WHERE m.RootProductId = @ProductId AND m.ProductId = child.ProductId AND m.BranchIds = bom.BranchIds)
                                            AND CHARINDEX(CONCAT('""', child.ProductId, '""'), bom.BranchIds, 0) <= 0
                        )
                        SELECT bom.*, p.ProductCode, p.ProductName, u.UnitName, CONVERT(BIT, CASE WHEN m.ProductId IS NOT NULL THEN 1 ELSE 0 END) AS IsMaterial
                        FROM prd_bom bom
                        LEFT JOIN ProductMaterial m ON m.RootProductId = @ProductId AND m.ProductId = bom.ChildProductId AND m.BranchIds = bom.BranchIds
                        LEFT JOIN Product p ON bom.ChildProductId = p.ProductId
                        LEFT JOIN ProductExtraInfo pei ON bom.ProductId = pei.ProductId
                        LEFT JOIN v_Unit u ON p.UnitId = u.F_Id;";

            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productId)
            };

            var resultData = await _stockDbContext.QueryDataTable(sql, parammeters);

            return resultData.ConvertData<ProductBomOutput>();
        }

        public async Task<bool> Update(int productId, IList<ProductBomInput> productBoms, IList<ProductMaterialModel> productMaterials)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            // Validate data
            // Validate child product id
            var childIds = productBoms.Select(b => b.ChildProductId).Distinct().ToList();
            if (_stockDbContext.Product.Count(p => childIds.Contains(p.ProductId)) != childIds.Count) throw new BadRequestException(ProductErrorCode.ProductNotFound, "Vật tư không tồn tại");

            if (productBoms.Any(p => p.ProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Vật tư không thuộc sản phẩm");

            // Validate materials
            if(productMaterials.Any(m => m.RootProductId != productId)) throw new BadRequestException(GeneralCode.InvalidParams, "Nguyên vật liệu không thuộc sản phẩm");

            // Remove duplicate
            productBoms = productBoms.GroupBy(b => new { b.ProductId, b.ChildProductId }).Select(g => g.First()).ToList();

            // Get old BOM info
            var oldBoms = _stockDbContext.ProductBom.Where(b => b.ProductId == productId).ToList();
            var newBoms = new List<ProductBomInput>(productBoms);
            var changeBoms = new List<(ProductBom OldValue, ProductBomInput NewValue)>();

            // Cập nhật BOM
            foreach (var newItem in productBoms)
            {
                var oldBom = oldBoms.FirstOrDefault(b => b.ChildProductId == newItem.ChildProductId);
                // Nếu là thay đổi
                if (oldBom != null)
                {
                    // Kiểm tra thay đổi thông tin
                    if (HasChange(oldBom, newItem))
                    {
                        changeBoms.Add((oldBom, newItem));
                    }
                    newBoms.Remove(newItem);
                    oldBoms.Remove(oldBom);
                }
            }

            // Xóa BOM
            foreach (var entity in oldBoms)
            {
                entity.IsDeleted = true;
            }

            // Tạo mới bom
            foreach (var newBom in newBoms)
            {
                var entity = _mapper.Map<ProductBom>(newBom);
                entity.ProductBomId = 0;
                _stockDbContext.ProductBom.Add(entity);
            }

            // Cập nhật BOM
            foreach (var updateBom in changeBoms)
            {
                updateBom.OldValue.Quantity = updateBom.NewValue.Quantity;
                updateBom.OldValue.Wastage = updateBom.NewValue.Wastage;
            }

            // Cập nhật Material
            var oldMaterials = _stockDbContext.ProductMaterial.Where(m => m.RootProductId == productId).ToList();
            var createMaterials = productMaterials
                .Where(nm => !oldMaterials.Any(om => om.ProductId == nm.ProductId && om.BranchIds == nm.BranchIds))
                .AsQueryable()
                .ProjectTo<ProductMaterial>(_mapper.ConfigurationProvider)
                .ToList();
            var deleteMaterials = oldMaterials
                .Where(om => !productMaterials.Any(nm => nm.ProductId == om.ProductId && nm.BranchIds == om.BranchIds))
                .ToList();

            _stockDbContext.ProductMaterial.RemoveRange(deleteMaterials);
            _stockDbContext.ProductMaterial.AddRange(createMaterials);

            await _stockDbContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.ProductBom, productId, $"Cập nhật chi tiết bom cho mặt hàng {product.ProductCode}, tên hàng {product.ProductName}", productBoms.JsonSerialize());
            return true;
        }

        private bool HasChange(ProductBom oldValue, ProductBomInput newValue)
        {
            return oldValue.Quantity != newValue.Quantity
                || oldValue.Wastage != newValue.Wastage;
        }
    }
}
