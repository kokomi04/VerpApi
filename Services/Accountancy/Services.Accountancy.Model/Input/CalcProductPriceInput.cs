using System.Collections.Generic;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProductPriceInput
    {
        public long Date { get; set; }
        public IList<int> ProductIds { get; set; }
    }
}
