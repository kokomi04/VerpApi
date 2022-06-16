using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ArrangeShift
    {
        public ArrangeShift()
        {
            ArrangeShiftItem = new HashSet<ArrangeShiftItem>();
        }

        public int ArrangeShiftId { get; set; }
        public int ArrangeShiftMode { get; set; }
        public int WorkScheduleId { get; set; }
        public int OrdinalNumber { get; set; }

        public virtual WorkSchedule WorkSchedule { get; set; }
        public virtual ICollection<ArrangeShiftItem> ArrangeShiftItem { get; set; }
    }
}
