using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryField : BaseEntity
    {
        public CategoryField()
        {
            DestCategoryFields = new HashSet<CategoryField>();
            DestCategoryTitleFields = new HashSet<CategoryField>();
        }

        public int CategoryFieldId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Sequence { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsShowList { get; set; }
        //public bool IsShowSearchTable { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public virtual DataType DataType { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<CategoryField> DestCategoryFields { get; set; }
        public virtual ICollection<CategoryField> DestCategoryTitleFields { get; set; }
        public virtual CategoryField SourceCategoryField { get; set; }
        public virtual CategoryField SourceCategoryTitleField { get; set; }
    }
}
