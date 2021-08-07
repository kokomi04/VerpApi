using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakeRepresentativeModel : IMapFrom<StockTakeRepresentative>
    {
        public long StockTakePeriodId { get; set; }
        public int UserId { get; set; }
    }
}