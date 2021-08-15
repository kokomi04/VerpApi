using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.StockTake;

namespace VErp.Services.Stock.Service.StockTake

{
    public interface IStockTakeService
    {

        Task<StockTakeModel> CreateStockTake(StockTakeModel model);

        Task<StockTakeModel> GetStockTake(long stockTakeId);

        Task<StockTakeModel> UpdateStockTake(long stockTakeId, StockTakeModel model);

        Task<bool> DeleteStockTake(long stockTakeId);

        Task<bool> ApproveStockTake(long stockTakeId);
    }
}
