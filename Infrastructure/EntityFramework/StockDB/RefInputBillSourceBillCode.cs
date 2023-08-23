using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class RefInputBillSourceBillCode
{
    public int InputTypeId { get; set; }

    public long InputBillFId { get; set; }

    public string SoCt { get; set; }

    public string InputTypeTitle { get; set; }

    public string SourceBillCode { get; set; }
}
