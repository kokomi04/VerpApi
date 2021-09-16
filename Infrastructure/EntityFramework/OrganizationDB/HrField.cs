using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class HrField
    {
        public HrField()
        {
            HrAreaField = new HashSet<HrAreaField>();
        }

        public int HrFieldId { get; set; }
        public int HrAreaId { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int DecimalPlace { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string Structure { get; set; }
        public bool IsReadOnly { get; set; }
        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public string OnClick { get; set; }
        public string ReferenceUrl { get; set; }
        public bool? IsImage { get; set; }

        public virtual HrArea HrArea { get; set; }
        public virtual ICollection<HrAreaField> HrAreaField { get; set; }
    }
}
