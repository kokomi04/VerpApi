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
				                        ChildProductId,
				                        1 AS Level,
				                        Quantity,
				                        Wastage
		                        FROM ProductBom
		                        WHERE ProductId = @ProductId AND IsDeleted = 0
		                        UNION ALL
		                        SELECT
				                        child.ProductBomId,
				                        child.ProductId, 
				                        child.ChildProductId,
				                        bom.Level + 1 AS Level,
				                        child.Quantity,
				                        child.Wastage
		                        FROM
				                        ProductBom child
				                        INNER JOIN prd_bom bom ON bom.ChildProductId = child.ProductId
				                        WHERE child.IsDeleted = 0 AND NOT EXISTS (SELECT 1 FROM ProductMaterial m WHERE m.RootProductId = @ProductId AND m.ProductId = child.ProductId)
                        )
                        SELECT bom.*, p.ProductCode, p.ProductName, u.UnitName, CONVERT(BIT, CASE WHEN m.ProductId IS NOT NULL THEN 1 ELSE 0 END) AS IsMaterial
                        FROM prd_bom bom
                        LEFT JOIN ProductMaterial m ON m.RootProductId = @ProductId AND m.ProductId = bom.ChildProductId
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

        public async Task<bool> Update(int productId, IList<ProductBomInput> req)
        {
            var product = _stockDbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            // Validate data
            var productIds = req.Select(b => b.ProductId).ToList();
            var childIds = req.Select(b => b.ChildProductId).ToList();
            var allProductIds = productIds.Union(childIds).Distinct().ToList();

            if (_stockDbContext.Product.Count(p => allProductIds.Contains(p.ProductId)) != allProductIds.Count) throw new BadRequestException(ProductErrorCode.ProductNotFound);

            // Validate product id
            if (productIds.Any(p => p != productId && !productIds.Contains(p))) throw new BadRequestException(GeneralCode.InvalidParams);

            // Validate duplicate
            if (req.GroupBy(b => new { b.ProductId, b.ChildProductId }).Any(g => g.Count() > 1))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin nguyên vật liệu bị trùng lặp");
            }

            // Get old BOM info
            var sql = @$"WITH prd_bom AS (
		                        SELECT
				                        ProductBomId,
				                        ProductId,
				                        ChildProductId
		                        FROM ProductBom
		                        WHERE ProductId = @ProductId AND IsDeleted = 0
		                        UNION ALL
		                        SELECT
				                        child.ProductBomId,
				                        child.ProductId, 
				                        child.ChildProductId
		                        FROM
				                        ProductBom child
				                        INNER JOIN prd_bom bom ON bom.ChildProductId = child.ProductId
				                        WHERE child.IsDeleted = 0
                        )
                        SELECT bom.*, CONVERT(BIT, CASE WHEN m.ProductId IS NOT NULL THEN 1 ELSE 0 END) AS IsMaterial FROM prd_bom bom
                        LEFT JOIN ProductMaterial m ON m.RootProductId = @ProductId AND m.ProductId = bom.ChildProductId;";
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", productId)
            };
            var resultData = await _stockDbContext.QueryDataTable(sql, parammeters);

            var oldBoms = resultData.ConvertData<ProductBomInput>();
            var newBoms = new List<ProductBomInput>(req);
            var changeBoms = new List<(ProductBomInput OldValue, ProductBomInput NewValue)>();

            // Xử lý loại bỏ thông tin BOM sản phẩm con những chi tiết sẽ đổi từ bán thành phẩm sang nguyên vật liệu trong danh sách cũ
            foreach (var newItem in req.Where(b => b.IsMaterial))
            {
                var oldBom = oldBoms.FirstOrDefault(b => b.ProductId == newItem.ProductId && b.ChildProductId == newItem.ChildProductId);
                if (!oldBom.IsMaterial)
                {
                    // Bỏ thông tin BOM sản phẩm con trong danh sách cũ
                    RemoveChildOfMaterial(ref oldBoms, oldBom.ChildProductId);
                }
            }

            foreach (var newItem in req)
            {
                var oldBom = oldBoms.FirstOrDefault(b => b.ProductId == newItem.ProductId && b.ChildProductId == newItem.ChildProductId);
                // Nếu là thay đổi
                if (oldBom != null)
                {
                    // Đổi từ bán thành phẩm sang nguyên vật liệu
                    if (newItem.IsMaterial && !oldBom.IsMaterial)
                    {
                        // Thêm thông tin nguyên vật liệu
                        _stockDbContext.ProductMaterial.Add(new ProductMaterial
                        {
                            ProductId = newItem.ChildProductId,
                            RootProductId = productId
                        });
                    }
                    // Đổi từ nguyên vật liệu sang bán thành phẩm
                    if (!newItem.IsMaterial && oldBom.IsMaterial)
                    {
                        // Xóa thông tin nguyên vật liệu
                        var productMaterial = _stockDbContext.ProductMaterial.First(m => m.ProductId == oldBom.ChildProductId && m.RootProductId == productId);
                        _stockDbContext.ProductMaterial.Remove(productMaterial);
                    }
                    // Kiểm tra thay đổi thông tin
                    if (HasChange(oldBom, newItem))
                    {
                        changeBoms.Add((oldBom, newItem));
                    }
                    newBoms.Remove(newItem);
                    oldBoms.Remove(oldBom);
                }
                else // Nếu thêm mới
                {
                    // Thêm mới chi tiết là nguyên vật liệu
                    if (newItem.IsMaterial)
                    {
                        // Thêm thông tin nguyên vật liệu
                        _stockDbContext.ProductMaterial.Add(new ProductMaterial
                        {
                            ProductId = newItem.ChildProductId,
                            RootProductId = productId
                        });
                    }
                }
            }

            // Kiểm tra danh sách bom bị xóa
            var deleteBomIds = new List<long>();
            if (oldBoms.Count > 0)
            {
                // Danh sách mới có BOM của ProductId trong danh sách mới hoặc ProductId của BOM là productId gốc thì là xóa
                foreach(var oldItem in oldBoms)
                {
                    if(oldItem.ProductId == productId && req.Any(b => b.ChildProductId == oldItem.ProductId))
                    {
                        deleteBomIds.Add(oldItem.ProductBomId.Value);
                    }
                }
                // Xóa bom
                var deleteBoms = _stockDbContext.ProductBom.Where(b => deleteBomIds.Contains(b.ProductBomId)).ToList();
                foreach (var deleteBom in deleteBoms)
                {
                    deleteBom.IsDeleted = true;
                }
                // Xóa material
                var childProductIds = deleteBoms.Select(b => b.ChildProductId).ToList();
                var deleteMaterials = _stockDbContext.ProductMaterial
                    .Where(m => m.RootProductId == productId && childProductIds.Contains(m.ProductId))
                    .ToList();
                _stockDbContext.ProductMaterial.RemoveRange(deleteMaterials);
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
            if (changeBoms.Count > 0)
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

        private void RemoveChildOfMaterial(ref List<ProductBomInput> oldBoms, int childProductId)
        {
            var childrent = oldBoms.Where(b => b.ProductId == childProductId);
            foreach (var child in childrent)
            {
                RemoveChildOfMaterial(ref oldBoms, child.ChildProductId);
            }
            oldBoms.RemoveAll(b => b.ProductId == childProductId);
        }

        private bool HasChange(ProductBomInput oldValue, ProductBomInput newValue)
        {
            return oldValue.Quantity != newValue.Quantity
                || oldValue.Wastage != newValue.Wastage;
        }

    }
}
