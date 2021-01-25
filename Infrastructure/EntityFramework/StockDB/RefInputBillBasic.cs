using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class RefInputBillBasic
    {
        public int InputTypeId { get; set; }
        public long InputBillFId { get; set; }
        public string SoCt { get; set; }
    }
}
