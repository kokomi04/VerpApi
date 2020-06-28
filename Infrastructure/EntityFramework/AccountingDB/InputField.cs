using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputField
    {
        public InputField()
        {
            InputAreaField = new HashSet<InputAreaField>();
        }

        public int InputFieldId { get; set; }
        public int FieldIndex { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual DataType DataType { get; set; }
        public virtual CategoryField ReferenceCategoryField { get; set; }
        public virtual CategoryField ReferenceCategoryTitleField { get; set; }
        public virtual ICollection<InputAreaField> InputAreaField { get; set; }
    }
}
