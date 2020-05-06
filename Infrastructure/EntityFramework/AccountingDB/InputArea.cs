using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputArea
    {
        public InputArea()
        {
            InputAreaField = new HashSet<InputAreaField>();
            InputValueRow = new HashSet<InputValueRow>();
        }

        public int InputAreaId { get; set; }
        public int InputTypeId { get; set; }
        public string InputAreaCode { get; set; }
        public string Title { get; set; }
        public bool IsMultiRow { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int Columns { get; set; }

        public virtual InputType InputType { get; set; }
        public virtual ICollection<InputAreaField> InputAreaField { get; set; }
        public virtual ICollection<InputValueRow> InputValueRow { get; set; }
    }
}
