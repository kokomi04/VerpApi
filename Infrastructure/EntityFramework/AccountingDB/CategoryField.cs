using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryField
    {
        public CategoryField()
        {
            CategoryRowValue = new HashSet<CategoryRowValue>();
            InputFieldReferenceCategoryField = new HashSet<InputField>();
            InputFieldReferenceCategoryTitleField = new HashSet<InputField>();
            InputTypeViewFieldReferenceCategoryField = new HashSet<InputTypeViewField>();
            InputTypeViewFieldReferenceCategoryTitleField = new HashSet<InputTypeViewField>();
            InverseReferenceCategoryField = new HashSet<CategoryField>();
            InverseReferenceCategoryTitleField = new HashSet<CategoryField>();
        }

        public int CategoryFieldId { get; set; }
        public int CategoryId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public string CategoryFieldName { get; set; }
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsShowList { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsTreeViewKey { get; set; }
        public bool? IsShowSearchTable { get; set; }
        public bool IsReadOnly { get; set; }
        public int CategoryAreaId { get; set; }

        public virtual Category Category { get; set; }
        public virtual CategoryArea CategoryArea { get; set; }
        public virtual DataType DataType { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual CategoryField ReferenceCategoryField { get; set; }
        public virtual CategoryField ReferenceCategoryTitleField { get; set; }
        public virtual ICollection<CategoryRowValue> CategoryRowValue { get; set; }
        public virtual ICollection<InputField> InputFieldReferenceCategoryField { get; set; }
        public virtual ICollection<InputField> InputFieldReferenceCategoryTitleField { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewFieldReferenceCategoryField { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewFieldReferenceCategoryTitleField { get; set; }
        public virtual ICollection<CategoryField> InverseReferenceCategoryField { get; set; }
        public virtual ICollection<CategoryField> InverseReferenceCategoryTitleField { get; set; }
    }
}
