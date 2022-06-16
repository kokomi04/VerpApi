using System.Threading.Tasks;
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
