using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class DashboardTypeView
    {
        public DashboardTypeView()
        {
            DashboardTypeViewField = new HashSet<DashboardTypeViewField>();
        }

        public int DashboardTypeViewId { get; set; }
        public string DashboardTypeViewName { get; set; }
        public int DashboardTypeId { get; set; }
        public bool IsDefault { get; set; }
        public int? SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual DashboardType DashboardType { get; set; }
        public virtual ICollection<DashboardTypeViewField> DashboardTypeViewField { get; set; }
    }
}
