using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class ReportType
    {
        public ReportType()
        {
            ReportTypeView = new HashSet<ReportTypeView>();
        }

        public int ReportTypeId { get; set; }
        public int ReportTypeGroupId { get; set; }
        public string ReportTypeName { get; set; }
        public string ReportPath { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ReportTypeGroup ReportTypeGroup { get; set; }
        public virtual ICollection<ReportTypeView> ReportTypeView { get; set; }
    }
}
