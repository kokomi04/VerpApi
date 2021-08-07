using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakeDetailModel : IMapFrom<StockTakeDetail>
    {
        public long StockTakeDetailId { get; set; }
        public long StockTakeId { get; set; }
        public int ProductId { get; set; }
        public int PackageId { get; set; }
        public decimal StockTakeQuantity { get; set; }
        public string Note { get; set; }
    }
}