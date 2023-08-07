using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB;

public partial class InputBill
{
    public long FId { get; set; }

    public int InputTypeId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int LatestBillVersion { get; set; }

    public int SubsidiaryId { get; set; }

    public string BillCode { get; set; }

    public long? ParentInputBillFId { get; set; }

    public virtual ICollection<InputBillAllocation> InputBillAllocation { get; set; } = new List<InputBillAllocation>();

    public virtual InputType InputType { get; set; }

    public virtual ICollection<InputBill> InverseParentInputBillF { get; set; } = new List<InputBill>();

    public virtual InputBill ParentInputBillF { get; set; }
}
