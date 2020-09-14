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
        public string MainView { get; set; }
        public string Joins { get; set; }
        public string Wheres { get; set; }
        public string OrderBy { get; set; }
        public string Head { get; set; }
        public string Footer { get; set; }
        public string HeadSql { get; set; }
        public string BodySql { get; set; }
        public string FooterSql { get; set; }
        public string PrintTitle { get; set; }
        public string GroupColumns { get; set; }
        public string Sign { get; set; }
        public string HtmlTemplate { get; set; }
        public int? DetailReportId { get; set; }
        public string DetailReportParams { get; set; }
        public string OnLoadJsCode { get; set; }
        public string PreLoadDataJsCode { get; set; }
        public string AfterLoadDataJsCode { get; set; }
        public string OnCloseJsCode { get; set; }
        public string Columns { get; set; }
        public bool IsBsc { get; set; }
        public string BscConfig { get; set; }
        public string HeadPrint { get; set; }

        public virtual ReportTypeGroup ReportTypeGroup { get; set; }
        public virtual ICollection<ReportTypeView> ReportTypeView { get; set; }
    }
}
