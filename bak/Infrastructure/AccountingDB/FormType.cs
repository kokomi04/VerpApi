using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class FormType
    {
        public FormType()
        {
            CategoryField = new HashSet<CategoryField>();
            InputTypeViewField = new HashSet<InputTypeViewField>();
        }

        public int FormTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<CategoryField> CategoryField { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewField { get; set; }
    }
}
