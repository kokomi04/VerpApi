using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryTrackingUpdateContext
    {
        public StockDBContext StockDbContext { get; set; }
        public Inventory InventoryInfo { get; set; }
        public InventoryChange InventoryChange { get; set; }

        public IList<InventoryDetailChange> InventoryDetailChanges { get; set; }
        public IDictionary<long, InventoryDetail> InventoryDetails { get; set; }

        public int StockId { get { return InventoryInfo.StockId; } }
        public long InventoryId { get { return InventoryInfo.InventoryId; } }

        public IDictionary<int, ProductChangeInfo> ProductChanges { get; set; }
        public EnumInventoryType InventoryTypeId { get { return (EnumInventoryType)InventoryInfo.InventoryTypeId; } }

        public EnumInventoryDateChangeType ChangeType
        {
            get
            {
                if (!InventoryChange.OldDate.HasValue) return EnumInventoryDateChangeType.New;
                return InventoryChange.OldDate.Value < InventoryInfo.Date ? EnumInventoryDateChangeType.Increase : EnumInventoryDateChangeType.Decrease;
            }
        }
        
    }


    public class ProductChangeInfo
    {
        public decimal TotalOldPrimaryQuantity { get; set; }
        public decimal TotalNewPrimaryQuantity { get; set; }
        public bool IsChange { get; set; }
        public decimal DeltaChange { get; set; }
    }

    public class InventoryTrackingUpdateContextBuilder
    {
        InventoryTrackingUpdateContext context;
        public InventoryTrackingUpdateContextBuilder()
        {
            context = new InventoryTrackingUpdateContext();
        }
        public InventoryTrackingUpdateContextBuilder StockDbContext(StockDBContext stockDbContext)
        {
            context.StockDbContext = stockDbContext;
            return this;
        }

        public InventoryTrackingUpdateContextBuilder InventoryInfo(Inventory inventoryInfo)
        {
            context.InventoryInfo = inventoryInfo;
            return this;
        }


        public async Task<InventoryTrackingUpdateContext> BuildAsync()
        {
            //1. Lấy thông tin tracking
            var inventoryChange = await context.StockDbContext.InventoryChange
                .Where(iv => iv.InventoryId == context.InventoryId)
                .FirstOrDefaultAsync();

            //2. Lấy dữ liệu cũ
            var oldData = await context.StockDbContext.InventoryDetailChange
                .Where(t => t.InventoryId == context.InventoryId)
                .ToListAsync();

            //3. Lấy dữ liệu mới
            var newData = await context.StockDbContext.InventoryDetail
                .IgnoreQueryFilters()
                .Where(t => t.InventoryId == context.InventoryId)
                .ToListAsync();

            context.InventoryChange = inventoryChange;
            context.InventoryDetailChanges = oldData;
            context.InventoryDetails = newData.ToDictionary(d => d.InventoryDetailId, d => d);

            var productChanges = new Dictionary<int, ProductChangeInfo>();
            foreach (var oldDetailGroup in oldData.GroupBy(d => d.ProductId))
            {
                var totalOldPrimaryQuantity = oldDetailGroup.Sum(d => d.OldPrimaryQuantity);

                var totalNewPrimaryQuantity = newData.Where(d => d.ProductId == oldDetailGroup.Key).Sum(d => d.PrimaryQuantity);

                var isChange = totalOldPrimaryQuantity != totalNewPrimaryQuantity;

                productChanges.Add(oldDetailGroup.Key, new ProductChangeInfo()
                {
                    TotalOldPrimaryQuantity = totalOldPrimaryQuantity,
                    TotalNewPrimaryQuantity = totalNewPrimaryQuantity,
                    IsChange = isChange,
                    DeltaChange = totalNewPrimaryQuantity - totalOldPrimaryQuantity
                });
            }

            context.ProductChanges = productChanges;
            return context;
        }
    }
}
