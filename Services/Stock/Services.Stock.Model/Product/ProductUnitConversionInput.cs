namespace VErp.Services.Stock.Model.Product
{
    public class ProductUnitConversionModel
    {
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public string FactorExpression { get; set; }
        public string ConversionDescription { get; set; }
    }
}
