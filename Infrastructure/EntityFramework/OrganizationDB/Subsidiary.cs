﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class Subsidiary
{
    public int SubsidiaryId { get; set; }

    public int? ParentSubsidiaryId { get; set; }

    public string SubsidiaryCode { get; set; }

    public string SubsidiaryName { get; set; }

    public string Address { get; set; }

    public string TaxIdNo { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string Fax { get; set; }

    public string Description { get; set; }

    public int SubsidiaryStatusId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<EmployeeSubsidiary> EmployeeSubsidiary { get; set; } = new List<EmployeeSubsidiary>();

    public virtual ICollection<Subsidiary> InverseParentSubsidiary { get; set; } = new List<Subsidiary>();

    public virtual Subsidiary ParentSubsidiary { get; set; }
}
