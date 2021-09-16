using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class HrArea
    {
        public HrArea()
        {
            HrAreaField = new HashSet<HrAreaField>();
            HrField = new HashSet<HrField>();
        }

        public int HrAreaId { get; set; }
        public int HrTypeId { get; set; }
        public string HrAreaCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsMultiRow { get; set; }
        public bool IsAddition { get; set; }
        public int Columns { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string ColumnStyles { get; set; }

        public virtual HrType HrType { get; set; }
        public virtual ICollection<HrAreaField> HrAreaField { get; set; }
        public virtual ICollection<HrField> HrField { get; set; }
    }
}
