using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProductPriceWeightedAverageInput
    {
        public long Date { get; set; }
        public IList<int> ProductIds { get; set; }
    }
}
