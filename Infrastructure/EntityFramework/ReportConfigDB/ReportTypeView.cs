using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class ReportTypeView
    {
        public ReportTypeView()
        {
            ReportTypeViewField = new HashSet<ReportTypeViewField>();
        }

        public int ReportTypeViewId { get; set; }
        public string ReportTypeViewName { get; set; }
        public int ReportTypeId { get; set; }
        public bool IsDefault { get; set; }
        public int? SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ReportType ReportType { get; set; }
        public virtual ICollection<ReportTypeViewField> ReportTypeViewField { get; set; }
    }
}
