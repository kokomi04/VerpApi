namespace VErp.Services.Stock.Model.StockTake
{
    public class StockRemainQuantity
    {
        public int ProductId { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal RemainQuantity { get; set; }
        public decimal ProductUnitConversionRemainQuantity { get; set; }
    }

    public class CalcStockRemainInputModel
    {
        public int[] ProductIds { get; set; }
        public long StockTakePeriodDate { get; set; }
        public int StockId { get; set; }
    }
}