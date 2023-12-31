﻿namespace VErp.Services.Stock.Model.Product.Pu
{
    public class ProductUnitConversionModel
    {
        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public string FactorExpression { get; set; }
        public string ConversionDescription { get; set; }
    }
}
