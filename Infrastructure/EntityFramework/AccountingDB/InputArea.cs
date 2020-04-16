using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputArea : BaseEntity
    {
        public InputArea()
        {
            InputAreaFields = new HashSet<InputAreaField>();
        }

        public int InputAreaId { get; set; }
        public int InputTypeId { get; set; }
        public string InputAreaCode { get; set; }
        public string Title { get; set; }
        public bool IsMultiRow { get; set; }

        public virtual InputType InputType { get; set; }

        public virtual ICollection<InputAreaField> InputAreaFields { get; set; }
    }
}
