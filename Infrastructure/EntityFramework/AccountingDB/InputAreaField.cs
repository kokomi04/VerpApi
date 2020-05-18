using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputAreaField
    {
        public int InputAreaFieldId { get; set; }
        public int InputAreaId { get; set; }
        public int FieldIndex { get; set; }
        public int InputTypeId { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRequire { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public string RegularExpression { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string Filters { get; set; }
        public string IsListFilter { get; set; }

        public virtual DataType DataType { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual InputArea InputArea { get; set; }
        public virtual InputType InputType { get; set; }
        public virtual CategoryField ReferenceCategoryField { get; set; }
        public virtual CategoryField ReferenceCategoryTitleField { get; set; }
        public virtual InputAreaFieldStyle InputAreaFieldStyle { get; set; }
    }
}
