﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class RefInputBillBasic
    {
        public int InputTypeId { get; set; }
        public long InputBillFId { get; set; }
        public string SoCt { get; set; }
        public string InputTypeTitle { get; set; }
    }
}
