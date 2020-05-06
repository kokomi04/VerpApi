using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputTypeView
    {
        public InputTypeView()
        {
            InputTypeViewField = new HashSet<InputTypeViewField>();
        }

        public int InputTypeViewId { get; set; }
        public int InputTypeId { get; set; }
        public int? UserId { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? CreatedDatetimeUtc { get; set; }
        public int? Columns { get; set; }

        public virtual InputType InputType { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewField { get; set; }
    }
}
