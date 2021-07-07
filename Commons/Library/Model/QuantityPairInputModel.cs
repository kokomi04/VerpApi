using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Library.Model
{
    public class QuantityPairInputModel
    {
        public decimal PrimaryQuantity { get; set; }
        public int PrimaryDecimalPlace { get; set; }

        public decimal PuQuantity { get; set; }
        public int PuDecimalPlace { get; set; }

        public string FactorExpression { get; set; }

        public decimal? FactorExpressionRate { get; set; }
    }
}
