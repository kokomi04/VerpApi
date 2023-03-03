using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryRefTable
    {
        public int SalaryRefTableId { get; set; }
        public int SortOrder { get; set; }
        public string FromField { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
