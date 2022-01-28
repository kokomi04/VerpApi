using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakeDetailModel : IMapFrom<StockTakeDetail>
    {
        public long StockTakeDetailId { get; set; }
        public long StockTakeId { get; set; }
        public int ProductId { get; set; }
        public long? PackageId { get; set; }
        public string PackageCode { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public string Note { get; set; }

        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
    }
}