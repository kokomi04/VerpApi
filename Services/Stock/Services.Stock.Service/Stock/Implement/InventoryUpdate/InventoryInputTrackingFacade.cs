using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryInputTrackingFacade : InventoryTrackingFacadeAbstract
    {
        public InventoryInputTrackingFacade(InventoryTrackingUpdateContext ctx) : base(ctx)
        {

        }

        public override async Task Execute()
        {
            var inventoryChange = _context.InventoryChange;

            var inventoryInfo = _context.InventoryInfo;

            if (_context.InventoryTypeId != EnumInventoryType.Input)
            {
                throw new NotSupportedException();
            }

            var changeType = _context.ChangeType;

            foreach (var oldDetailGroup in _context.InventoryDetailChanges.GroupBy(d => d.ProductId))
            {
                _context.ProductChanges.TryGetValue(oldDetailGroup.Key, out var productChange);

                switch (changeType)
                {
                    case EnumInventoryDateChangeType.New:
                        await InputAddNewInventoryEvent(oldDetailGroup, productChange);
                        break;
                    case EnumInventoryDateChangeType.Increase:
                        await InputInventoryIncreaseDateEvent(oldDetailGroup, productChange);
                        break;
                    case EnumInventoryDateChangeType.Decrease:
                        await InputInventoryDecreaseDateEvent(oldDetailGroup, productChange);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }


            await SaveChange();
        }

        private async Task InputAddNewInventoryEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {

            //Step1: Cập nhật số dư hiện thời của phiếu

            //Cập nhật số dư hiện tại
            await UpdateCurrentInventoryBalance(oldDetailGroup);

            //Step2: Cộng dồn số lượng từ thời điểm của đơn đến hiện tại
            await InventoryInputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange, productChange.PuChanges.ToDictionary(c=>c.Key, c=>c.Value.DeltaPuChange));
        }

        private async Task InputInventoryIncreaseDateEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {
            //1.2. Nếu có thay đổi thời gian hoặc có thay đổi số lượng
            if (_context.InventoryInfo.Date != _context.InventoryChange.OldDate.Value || productChange.IsChange)
            {
                //Step1. Nếu đổi thời gian tăng lên: Giảm số lượng cũ từ oldDate -> newDate
                if (_context.InventoryInfo.Date > _context.InventoryChange.OldDate.Value)
                {
                    await InventoryInputBetweenAdd(oldDetailGroup.Key, -productChange.TotalOldPrimaryQuantity, productChange.PuChanges.ToDictionary(c => c.Key, c => -c.Value.TotalOldPuQuantity));
                }

                //Step2: Cập nhật số dư hiện thời của phiếu

                //Cập nhật số dư hiện tại
                await UpdateCurrentInventoryBalance(oldDetailGroup);


                //Step3: Cộng dồn số lượng từ thời điểm của đơn đến hiện tại
                await InventoryInputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange, productChange.PuChanges.ToDictionary(c => c.Key, c => c.Value.DeltaPuChange));
            }
        }

        private async Task InputInventoryDecreaseDateEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {
            //Step1: Cập nhật số dư hiện thời của phiếu

            //Cập nhật số dư hiện tại
            await UpdateCurrentInventoryBalance(oldDetailGroup);

            //Step2. Cộng số lượng từ newDate -> oldDate
            await InventoryInputBetweenAdd(oldDetailGroup.Key, productChange.TotalNewPrimaryQuantity, productChange.PuChanges.ToDictionary(c => c.Key, c => c.Value.TotalNewPuQuantity));

            //4.1.d Cộng số lượng oldDate => Now bằng số tăng lên (hoặc giảm đi)
            await InventoryInputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange, productChange.PuChanges.ToDictionary(c => c.Key, c => c.Value.DeltaPuChange));
        }

        protected override async Task<decimal> GetBeforeBalance(int productId)
        {

            var primaryQuantityRemaningSameTimeBefore = await (
             from d in _context.StockDbContext.InventoryDetail
             join iv in _context.StockDbContext.Inventory on d.InventoryId equals iv.InventoryId
             where iv.IsApproved
             && iv.StockId == _context.StockId
             && d.ProductId == productId
             && iv.Date == _context.InventoryInfo.Date
             && iv.InventoryTypeId == (int)EnumInventoryType.Input
             && iv.InventoryId < _context.InventoryId//Cùng thời điểm nhưng là phiếu nhập và vào trước

             orderby iv.Date descending, iv.InventoryId descending, d.InventoryDetailId descending
             select d.PrimaryQuantityRemaning
            ).FirstOrDefaultAsync();


            if (primaryQuantityRemaningSameTimeBefore > 0) return primaryQuantityRemaningSameTimeBefore.Value;

            return await GetBeforeDateBalance(productId);
        }


        protected override async Task<decimal> GetBeforePu(int productId, int productUnitConversionId)
        {

            var puRemaningSameTimeBefore = await (
             from d in _context.StockDbContext.InventoryDetail
             join iv in _context.StockDbContext.Inventory on d.InventoryId equals iv.InventoryId
             where iv.IsApproved
             && iv.StockId == _context.StockId
             && d.ProductId == productId
             && d.ProductUnitConversionId == productUnitConversionId
             && iv.Date == _context.InventoryInfo.Date
             && iv.InventoryTypeId == (int)EnumInventoryType.Input
             && iv.InventoryId < _context.InventoryId//Cùng thời điểm nhưng là phiếu nhập và vào trước

             orderby iv.Date descending, iv.InventoryId descending, d.InventoryDetailId descending
             select d.ProductUnitConversionQuantityRemaning
            ).FirstOrDefaultAsync();


            if (puRemaningSameTimeBefore > 0) return puRemaningSameTimeBefore.Value;

            return await GetBeforeDatePu(productId, productUnitConversionId);
        }



        private async Task InventoryInputBetweenAdd(int productId, decimal primaryQuantity, Dictionary<int, decimal> addPuQuantities)
        {
            //1. Cùng ngày from: Phiếu nhập có ID > ID hiện hành hoặc là phiếu xuất

            //2. Giữa khoảng thời gian 

            //3. Cùng ngày to: Phiếu nhập có ID < ID hiện hành

            var dateRangeCondition = $@"
        iv.Date = @FromDate AND (iv.InventoryTypeId = @InputInventoryTypeId AND iv.InventoryId > @InventoryId OR iv.InventoryTypeId = @OutputInventoryTypeId)
        OR iv.Date > @FromDate AND iv.Date < @ToDate
        OR iv.Date = @ToDate AND iv.InventoryTypeId = @InputInventoryTypeId AND iv.InventoryId < @InventoryId
";
            await UpdateInventoryDetailByCondition(dateRangeCondition, productId, primaryQuantity, addPuQuantities);
        }

        private async Task InventoryInputUpdateToNow(int productId, decimal deltaChangePrimaryQuantity, Dictionary<int, decimal> addPuQuantities)
        {
            //1. Cùng ngày: Phiếu nhập có ID > ID hiện hành hoặc là phiếu xuất

            //2. Hoặc ngày sau

            var dateRangeCondition = $@"
       iv.Date = @ToDate AND (iv.InventoryTypeId = @InputInventoryTypeId AND iv.InventoryId > @InventoryId OR iv.InventoryTypeId = @OutputInventoryTypeId)
        OR iv.Date > @ToDate
";
            await UpdateInventoryDetailByCondition(dateRangeCondition, productId, deltaChangePrimaryQuantity, addPuQuantities);
        }

    }
}
