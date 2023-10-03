using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB;

public partial class InputBillAllocation
{
    public long ParentInputBillFId { get; set; }

    public string DataAllowcationBillCode { get; set; }

    public virtual InputBill ParentInputBillF { get; set; }
}
