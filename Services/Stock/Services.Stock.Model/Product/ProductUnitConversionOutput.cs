namespace VErp.Services.Stock.Model.Product
{
    public class ProductUnitConversionOutput
    {
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public string FactorExpression { get; set; }
        public string ConversionDescription { get; set; }
    }
}
