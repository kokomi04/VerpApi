using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class DashboardType
    {
        public DashboardType()
        {
            DashboardTypeView = new HashSet<DashboardTypeView>();
        }

        public int DashboardTypeId { get; set; }
        public int DashboardTypeGroupId { get; set; }
        public string DashboardTypeName { get; set; }
        public string BodySql { get; set; }
        public string Columns { get; set; }
        public int SortOrder { get; set; }
        public string JsProcessedChart { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual DashboardTypeGroup DashboardTypeGroup { get; set; }
        public virtual ICollection<DashboardTypeView> DashboardTypeView { get; set; }
    }
}
