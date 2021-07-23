using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CategoryField
    {
        public int CategoryFieldId { get; set; }
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
        public bool IsDeleted { get; set; }
        public bool IsShowList { get; set; }
        public string RegularExpression { get; set; }
        public string Filters { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsShowSearchTable { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsTreeViewKey { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public int DecimalPlace { get; set; }
        public string DefaultValue { get; set; }
        public bool? IsImage { get; set; }

        public virtual Category Category { get; set; }
    }
}
