using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryOutTrackingFacade : InventoryTrackingFacadeAbstract
    {
        public InventoryOutTrackingFacade(InventoryTrackingUpdateContext ctx) : base(ctx)
        {

        }

        public override async Task Execute()
        {
            var inventoryChange = _context.InventoryChange;

            var inventoryInfo = _context.InventoryInfo;

            if (_context.InventoryTypeId != EnumInventoryType.Output)
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
                        await OutputAddNewInventoryEvent(oldDetailGroup, productChange);
                        break;
                    case EnumInventoryDateChangeType.Increase:
                        await OutputInventoryIncreaseDateEvent(oldDetailGroup, productChange);
                        break;
                    case EnumInventoryDateChangeType.Decrease:
                        await OutputInventoryDecreaseDateEvent(oldDetailGroup, productChange);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }


            await SaveChange();
        }

        private async Task OutputAddNewInventoryEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {

            //Step1: Cập nhật số dư hiện thời của phiếu

            //Cập nhật số dư hiện tại
            await UpdateCurrentInventoryBalance(oldDetailGroup);

            //Step2: Trừ dồn số lượng từ thời điểm của đơn đến hiện tại
            await InventoryOutputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange);
        }

        private async Task OutputInventoryIncreaseDateEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {
            //1.2. Nếu có thay đổi thời gian hoặc có thay đổi số lượng
            if (_context.InventoryInfo.Date != _context.InventoryChange.OldDate.Value || productChange.IsChange)
            {
                //Step1. Nếu đổi thời gian tăng lên: Tăng số lượng cũ từ oldDate -> newDate
                if (_context.InventoryInfo.Date > _context.InventoryChange.OldDate.Value)
                {
                    await InventoryOutputBetweenAdd(oldDetailGroup.Key, productChange.TotalOldPrimaryQuantity);
                }

                //Step2: Cập nhật số dư hiện thời của phiếu

                //Cập nhật số dư hiện tại
                await UpdateCurrentInventoryBalance(oldDetailGroup);


                //Step3: Trừ dồn số lượng từ thời điểm của đơn đến hiện tại
                await InventoryOutputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange);
            }
        }

        private async Task OutputInventoryDecreaseDateEvent(IGrouping<int, InventoryDetailChange> oldDetailGroup, ProductChangeInfo productChange)
        {
            //Step1: Cập nhật số dư hiện thời của phiếu

            //Cập nhật số dư hiện tại
            await UpdateCurrentInventoryBalance(oldDetailGroup);

            //Step2. Giảm số lượng từ newDate -> oldDate
            await InventoryOutputBetweenAdd(oldDetailGroup.Key, -productChange.TotalNewPrimaryQuantity);

            //Step3: Trừ dồn số lượng oldDate => Now bằng số tăng lên (hoặc giảm đi)
            await InventoryOutputUpdateToNow(oldDetailGroup.Key, productChange.DeltaChange);
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
              && (iv.InventoryTypeId == (int)EnumInventoryType.Input || iv.InventoryTypeId == (int)EnumInventoryType.Output && iv.InventoryId < _context.InventoryId)//Hoặc phiếu nhập cùng thời điểm hoặc phiếu xuất vào trước


              orderby iv.Date descending, iv.InventoryTypeId descending, iv.InventoryId descending, d.InventoryDetailId descending
              select d.PrimaryQuantityRemaning
                             ).FirstOrDefaultAsync();

            if (primaryQuantityRemaningSameTimeBefore > 0) return primaryQuantityRemaningSameTimeBefore.Value;

            return await GetBeforeDateBalance(productId);
        }



        private async Task InventoryOutputBetweenAdd(int productId, decimal primaryQuantity)
        {
            //1. Cùng ngày from: Phiếu xuất có ID > ID hiện hành

            //2. Giữa khoảng thời gian 

            //3. Cùng ngày to: Phiếu xuất có ID < ID hiện hành hoặc phiếu nhập

            var dateRangeCondition = $@"
        iv.Date = @FromDate AND iv.InventoryTypeId = @OutputInventoryTypeId AND iv.InventoryId > @InventoryId
        OR iv.Date > @FromDate AND iv.Date < @ToDate
        OR iv.Date = @ToDate AND (iv.InventoryTypeId = @OutputInventoryTypeId AND iv.InventoryId < @InventoryId OR iv.InventoryTypeId = @InputInventoryTypeId)
";
            await UpdateInventoryDetailByCondition(dateRangeCondition, productId, primaryQuantity);
        }

        private async Task InventoryOutputUpdateToNow(int productId, decimal deltaChangePrimaryQuantity)
        {

            //1. Cùng ngày: Phiếu xuất có ID > ID hiện hành
            //2. Hoặc ngày sau

            var dateRangeCondition = $@"
iv.Date = @ToDate AND (iv.InventoryTypeId = @OutputInventoryTypeId AND iv.InventoryId > @InventoryId)
OR iv.Date > @ToDate
";
            await UpdateInventoryDetailByCondition(dateRangeCondition, productId, -deltaChangePrimaryQuantity);
        }

    }
}
