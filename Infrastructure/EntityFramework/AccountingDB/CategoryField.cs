﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryField
    {
        public CategoryField()
        {
            DestCategoryFields = new HashSet<CategoryField>();
            DestCategoryTitleFields = new HashSet<CategoryField>();
            CategoryRowValues = new HashSet<CategoryRowValue>();
            CategoryValues = new HashSet<CategoryValue>();
            InputAreaFields = new HashSet<InputAreaField>();
            InputAreaTitleFields = new HashSet<InputAreaField>();
        }

        public int CategoryFieldId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public int CategoryId { get; set; }
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
        public bool IsShowList { get; set; }
        public bool IsShowSearchTable { get; set; }
        public bool IsTreeViewKey { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual DataType DataType { get; set; }
        public virtual FormType FormType { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<CategoryField> DestCategoryFields { get; set; }
        public virtual ICollection<CategoryField> DestCategoryTitleFields { get; set; }
        public virtual ICollection<CategoryRowValue> CategoryRowValues { get; set; }
        public virtual ICollection<CategoryValue> CategoryValues { get; set; }
        public virtual CategoryField SourceCategoryField { get; set; }
        public virtual CategoryField SourceCategoryTitleField { get; set; }

        public virtual ICollection<InputAreaField> InputAreaFields { get; set; }
        public virtual ICollection<InputAreaField> InputAreaTitleFields { get; set; }
    }
}
