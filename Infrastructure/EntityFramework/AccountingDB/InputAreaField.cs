using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputAreaField : BaseEntity
    {
        public InputAreaField()
        {
        }

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

        public virtual InputArea InputArea { get; set; }
        public virtual DataType DataType { get; set; }
        public virtual FormType FormType { get; set; }

        public virtual CategoryField SourceCategoryField { get; set; }
        public virtual CategoryField SourceCategoryTitleField { get; set; }

        public virtual InputAreaFieldStyle InputAreaFieldStyle { get; set; }
    }
}
