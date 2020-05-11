using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public interface IInventoryTrackingFacade
    {
        Task Execute();
    }

    public abstract class InventoryTrackingFacadeAbstract : IInventoryTrackingFacade
    {
        protected InventoryTrackingUpdateContext _context;

        public InventoryTrackingFacadeAbstract(InventoryTrackingUpdateContext ctx)
        {
            _context = ctx;
        }

        protected async Task SaveChange()
        {
            _context.InventoryChange.OldDate = _context.InventoryInfo.Date;
            _context.InventoryChange.IsSync = true;
            _context.InventoryChange.LastSyncTime = DateTime.UtcNow;
            await _context.StockDbContext.SaveChangesAsync();
        }

        protected async Task UpdateCurrentInventoryBalance(IGrouping<int, InventoryDetailChange> oldDetailGroup)
        {
            var beforeBalance = await GetBeforeBalance(oldDetailGroup.Key);

            var newData = _context.InventoryDetails;

            //Cập nhật số dư hiện tại
            foreach (var oldDetail in oldDetailGroup.OrderBy(d => d.InventoryDetailId))
            {
                if (newData.TryGetValue(oldDetail.InventoryDetailId, out var newDetail))
                {
                    if (_context.InventoryTypeId == EnumInventoryType.Input)
                    {
                        beforeBalance += newDetail.PrimaryQuantity;
                    }
                    else
                    {
                        beforeBalance -= newDetail.PrimaryQuantity;
                    }

                    newDetail.PrimaryQuantityRemaning = beforeBalance;

                    oldDetail.OldPrimaryQuantity = newDetail.PrimaryQuantity;
                }
                else
                {
                    oldDetail.OldPrimaryQuantity = 0;
                }
            }
        }

        protected async Task UpdateInventoryDetailByCondition(string dateRangeCondition, int productId, decimal addPrimaryQuantity)
        {
            var sql = $@"
UPDATE d
    SET d.PrimaryQuantityRemaning = d.PrimaryQuantityRemaning + @AddPrimaryQuantity
FROM
	dbo.InventoryDetail AS d
	JOIN dbo.Inventory iv ON d.InventoryId = iv.InventoryId
	WHERE 
	iv.IsDeleted=0 
	AND d.IsDeleted=0 
	AND iv.IsApproved = 1
	AND iv.StockId = @StockId
	AND d.ProductId = @ProductId
    AND ({dateRangeCondition})
 ";

#pragma warning disable EF1000 // Possible SQL injection vulnerability.

            var fromDate = _context.InventoryChange.OldDate ?? _context.InventoryInfo.Date;
            var toDate = _context.InventoryInfo.Date;

            if (fromDate > toDate)
            {
                var tg = fromDate;
                fromDate = toDate;
                toDate = tg;
            }

            await _context.StockDbContext.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@AddPrimaryQuantity", SqlDbType.Decimal) { Value = addPrimaryQuantity },
                new SqlParameter("@StockId", SqlDbType.Int) { Value = _context.StockId },
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId },
                new SqlParameter("@OldDate", SqlDbType.DateTime2) { IsNullable = true, Value = _context.InventoryChange.OldDate.HasValue ? (object)_context.InventoryChange.OldDate.Value : (object)DBNull.Value },
                new SqlParameter("@NewDate", SqlDbType.DateTime2) { Value = _context.InventoryInfo.Date },
                new SqlParameter("@FromDate", SqlDbType.DateTime2) { Value = fromDate },
                new SqlParameter("@ToDate", SqlDbType.DateTime2) { Value = toDate },
                new SqlParameter("@InputInventoryTypeId", SqlDbType.Int) { Value = (int)EnumInventoryType.Input },
                new SqlParameter("@OutputInventoryTypeId", SqlDbType.Int) { Value = (int)EnumInventoryType.Output },
                new SqlParameter("@InventoryId", SqlDbType.BigInt) { Value = _context.InventoryId }
                );
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
        }

        public abstract Task Execute();

        protected async Task<decimal> GetBeforeDateBalance(int productId)
        {
            
            var beforePrimaryQuantityRemaning = await (
             from d in _context.StockDbContext.InventoryDetail
             join iv in _context.StockDbContext.Inventory on d.InventoryId equals iv.InventoryId
             where iv.IsApproved
             && iv.StockId == _context.StockId
             && d.ProductId == productId

             && iv.Date < _context.InventoryInfo.Date//Phiếu trước thời điểm đó

             orderby iv.Date descending, iv.InventoryTypeId descending, iv.InventoryId descending, d.InventoryDetailId descending
             select d.PrimaryQuantityRemaning
            ).FirstOrDefaultAsync();

            decimal primaryQuantityRemaning = 0;

            if (beforePrimaryQuantityRemaning.HasValue)
            {
                primaryQuantityRemaning = beforePrimaryQuantityRemaning.Value;
            }

            return primaryQuantityRemaning;
        }

        /// <summary>
        /// Sort cho phép cùng thời điểm có thể lẫn lộn phiếu nhập/xuất, ưu tiên nhập trước, xuất sau để đảm bảo xuất được
        ///         Date
        ///             InputInventoryType
        ///                 InventoryId
        ///                     InventoryDetailId
        ///             OutputInventoryType
        ///                 InventoryId
        ///                     InventoryDetailId
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        protected abstract Task<decimal> GetBeforeBalance(int productId);
    }

    public class InventoryTrackingFacadeFactory
    {
        public static async Task<IInventoryTrackingFacade> Create(StockDBContext _stockDbContext, long inventoryId)
        {
            var inventoryInfo = await _stockDbContext.Inventory
                .IgnoreQueryFilters()
                .Where(iv => iv.IsApproved && iv.InventoryId == inventoryId)
                .FirstOrDefaultAsync();

            if (inventoryInfo == null)
            {
                throw new Exception("inventoryInfo not found!");
            }

            var ctx = await new InventoryTrackingUpdateContextBuilder()
               .StockDbContext(_stockDbContext)
               .InventoryInfo(inventoryInfo)
               .BuildAsync();

            switch ((EnumInventoryType)inventoryInfo.InventoryTypeId)
            {
                case EnumInventoryType.Input:
                    return new InventoryInputTrackingFacade(ctx);
                case EnumInventoryType.Output:
                    return new InventoryOutTrackingFacade(ctx);
                default:
                    throw new NotSupportedException("Invalid inventory type " + inventoryInfo.InventoryTypeId);
            }
        }
    }
}
