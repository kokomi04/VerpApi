using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    /*
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
                return InventoryInfo.Date >= InventoryChange.OldDate.Value ? EnumInventoryDateChangeType.Increase : EnumInventoryDateChangeType.Decrease;
            }
        }

    }


    public class ProductChangeInfo
    {
        public decimal TotalOldPrimaryQuantity { get; set; }
        public decimal TotalNewPrimaryQuantity { get; set; }

        public bool IsChange { get; set; }
        public decimal DeltaChange { get; set; }

        public Dictionary<int, PuChangeInfo> PuChanges { get; set; }
    }

    public class PuChangeInfo
    {
        public decimal TotalOldPuQuantity { get; set; }
        public decimal TotalNewPuQuantity { get; set; }

        public decimal DeltaPuChange { get; set; }
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
            var oldInventoryDetails = await context.StockDbContext.InventoryDetailChange
                .Where(t => t.InventoryId == context.InventoryId && !t.IsDeleted)
                .ToListAsync();

            //3. Lấy dữ liệu mới
            var newInventoryDetails = await context.StockDbContext.InventoryDetail
                .Where(t => t.InventoryId == context.InventoryId && !t.IsDeleted)
                .ToListAsync();

            //4. Ensure dữ liệu cũ
            if (inventoryChange == null)
            {
                inventoryChange = new InventoryChange()
                {
                    InventoryId = context.InventoryId,
                    IsSync = false,
                    LastSyncTime = DateTime.UtcNow,
                    OldDate = null
                };

                await context.StockDbContext.InventoryChange.AddAsync(inventoryChange);
            }


            var oldDataDic = oldInventoryDetails.ToDictionary(d => d.InventoryDetailId, d => d);
            var newOldDatas = new List<InventoryDetailChange>();
            foreach (var inventoryDetail in newInventoryDetails)
            {
                if (!oldDataDic.ContainsKey(inventoryDetail.InventoryDetailId))
                {
                    newOldDatas.Add(new InventoryDetailChange()
                    {
                        InventoryDetailId = inventoryDetail.InventoryDetailId,
                        InventoryId = context.InventoryId,
                        StockId = context.StockId,
                        OldPrimaryQuantity = 0,
                        OldPuConversionQuantity = 0,
                        IsDeleted = false,
                        ProductId = inventoryDetail.ProductId,
                        ProductUnitConversionId = inventoryDetail.ProductUnitConversionId
                    });
                }
            }

            if (newOldDatas.Count > 0)
            {
                await context.StockDbContext.InventoryDetailChange.AddRangeAsync(newOldDatas);
                oldInventoryDetails.AddRange(newOldDatas);
            }
            await context.StockDbContext.SaveChangesAsync();


            //5. Assign to context
            context.InventoryChange = inventoryChange;
            context.InventoryDetailChanges = oldInventoryDetails;
            context.InventoryDetails = newInventoryDetails.ToDictionary(d => d.InventoryDetailId, d => d);

            var productChanges = new Dictionary<int, ProductChangeInfo>();
            foreach (var oldDetailGroup in oldInventoryDetails.GroupBy(d => d.ProductId))
            {
                var totalOldPrimaryQuantity = oldDetailGroup.Sum(d => d.OldPrimaryQuantity);

                var totalNewPrimaryQuantity = newInventoryDetails.Where(d => d.ProductId == oldDetailGroup.Key).Sum(d => d.PrimaryQuantity);

                var isChange = totalOldPrimaryQuantity != totalNewPrimaryQuantity;

                var puChanges = new Dictionary<int, PuChangeInfo>();
                foreach (var g in oldDetailGroup.GroupBy(p => p.ProductUnitConversionId))
                {
                    var totalOldPuQuantity = g.Sum(d => d.OldPuConversionQuantity);

                    var totalNewPuQuantity = newInventoryDetails.Where(d => d.ProductId == oldDetailGroup.Key && d.ProductUnitConversionId == g.Key).Sum(d => d.ProductUnitConversionQuantity);

                    var deltaPuChange = totalNewPuQuantity - totalOldPuQuantity;

                    puChanges.Add(g.Key, new PuChangeInfo()
                    {
                        TotalOldPuQuantity = totalOldPuQuantity,
                        TotalNewPuQuantity = totalNewPuQuantity,
                        DeltaPuChange = deltaPuChange
                    });
                }

                productChanges.Add(oldDetailGroup.Key, new ProductChangeInfo()
                {
                    TotalOldPrimaryQuantity = totalOldPrimaryQuantity,
                    TotalNewPrimaryQuantity = totalNewPrimaryQuantity,

                    IsChange = isChange,
                    DeltaChange = totalNewPrimaryQuantity - totalOldPrimaryQuantity,

                    PuChanges = puChanges
                });
            }

            context.ProductChanges = productChanges;
            return context;
        }
    }*/
}
