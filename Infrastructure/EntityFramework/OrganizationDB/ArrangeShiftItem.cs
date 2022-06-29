using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ArrangeShiftItem
    {
        public ArrangeShiftItem()
        {
            InverseParentArrangeShiftItem = new HashSet<ArrangeShiftItem>();
        }

        public int ArrangeShiftItemId { get; set; }
        public int ArrangeShiftId { get; set; }
        public int? ShiftConfigurationId { get; set; }
        public int? OrdinalNumber { get; set; }
        public int? ParentArrangeShiftItemId { get; set; }

        public virtual ArrangeShift ArrangeShift { get; set; }
        public virtual ArrangeShiftItem ParentArrangeShiftItem { get; set; }
        public virtual ShiftConfiguration ShiftConfiguration { get; set; }
        public virtual ICollection<ArrangeShiftItem> InverseParentArrangeShiftItem { get; set; }
    }
}
