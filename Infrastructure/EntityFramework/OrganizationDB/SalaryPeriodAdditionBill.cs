﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriodAdditionBill
{
    public long SalaryPeriodAdditionBillId { get; set; }

    public int SubsidiaryId { get; set; }

    public int SalaryPeriodAdditionTypeId { get; set; }

    public string BillCode { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public string Content { get; set; }

    public DateTime Date { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SalaryPeriodAdditionBillEmployee> SalaryPeriodAdditionBillEmployee { get; set; } = new List<SalaryPeriodAdditionBillEmployee>();

    public virtual SalaryPeriodAdditionType SalaryPeriodAdditionType { get; set; }
}
