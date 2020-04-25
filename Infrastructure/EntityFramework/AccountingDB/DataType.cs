using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class DataType
    {
        public DataType()
        {
            CategoryFields = new HashSet<CategoryField>();
            InputAreaFields = new HashSet<InputAreaField>();
        }

        public int DataTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int DataSizeDefault { get; set; }

        public string RegularExpression { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<CategoryField> CategoryFields { get; set; }
        public virtual ICollection<InputAreaField> InputAreaFields { get; set; }
    }
}
