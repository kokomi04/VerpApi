using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ArrangeShiftItem
{
    public int ArrangeShiftItemId { get; set; }

    public int ArrangeShiftId { get; set; }

    public int? ShiftConfigurationId { get; set; }

    public int? OrdinalNumber { get; set; }

    public int? ParentArrangeShiftItemId { get; set; }

    public virtual ArrangeShift ArrangeShift { get; set; }

    public virtual ICollection<ArrangeShiftItem> InverseParentArrangeShiftItem { get; set; } = new List<ArrangeShiftItem>();

    public virtual ArrangeShiftItem ParentArrangeShiftItem { get; set; }

    public virtual ShiftConfiguration ShiftConfiguration { get; set; }
}
