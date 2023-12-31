﻿namespace VErp.Services.Stock.Model.Product.Pu
{
    public class ProductUnitConversionOutput
    {
        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }

        public string SecondaryUnitName { get; set; }

        public string FactorExpression { get; set; }

        public string ConversionDescription { get; set; }

        public bool IsDefault { get; set; }
        public int DecimalPlace { get; set; }
    }
}
