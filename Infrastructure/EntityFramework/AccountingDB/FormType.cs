using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class FormType 
    {
        public FormType()
        {
            CategoryFields = new HashSet<CategoryField>();
            InputAreaFields = new HashSet<InputAreaField>();
        }

        public int FormTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
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
