using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class DataType
    {
        public DataType()
        {
            CategoryField = new HashSet<CategoryField>();
            InputField = new HashSet<InputField>();
            InputTypeViewField = new HashSet<InputTypeViewField>();
        }

        public int DataTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int? DataSizeDefault { get; set; }
        public string RegularExpression { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<CategoryField> CategoryField { get; set; }
        public virtual ICollection<InputField> InputField { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewField { get; set; }
    }
}
