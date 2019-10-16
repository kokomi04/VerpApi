using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductUnitConversion
    {
        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public string FactorExpression { get; set; }
        public string ConversionDescription { get; set; }

        public virtual Product Product { get; set; }
    }
}
