using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class DataType : BaseEntity
    {
        public DataType()
        {
            CategoryFields = new HashSet<CategoryField>();
        }

        public int DataTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int DataSizeDefault { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<CategoryField> CategoryFields { get; set; }

    }
}
