using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class ReportTypeViewFieldValue
    {
        public int ReportTypeViewFieldId { get; set; }
        public int SubsidiaryId { get; set; }
        public string JsonValue { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
