using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class FormType : BaseEntity
    {
        public FormType()
        {
            CategoryFields = new HashSet<CategoryField>();
        }

        public int FormTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<CategoryField> CategoryFields { get; set; }
    }
}
