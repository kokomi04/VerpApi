using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryGroupField
{
    public int SalaryGroupId { get; set; }

    public int SalaryFieldId { get; set; }

    public string GroupName { get; set; }

    public string Title { get; set; }

    public bool IsEditable { get; set; }

    public bool IsHidden { get; set; }

    public int SortOrder { get; set; }

    public bool IsGroupRow { get; set; }

    public int GroupRowSortOrder { get; set; }

    public virtual SalaryField SalaryField { get; set; }

    public virtual SalaryGroup SalaryGroup { get; set; }
}
