using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputType
    {
        public InputType()
        {
            InputAreas = new HashSet<InputArea>();
            InputValueBills = new HashSet<InputValueBill>();
        }

        public int InputTypeId { get; set; }
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<InputArea> InputAreas { get; set; }
        public virtual ICollection<InputValueBill> InputValueBills { get; set; }

    }
}
