using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class InputTypeView
    {
        public InputTypeView()
        {
            InputTypeViewField = new HashSet<InputTypeViewField>();
        }

        public int InputTypeViewId { get; set; }
        public string InputTypeViewName { get; set; }
        public int InputTypeId { get; set; }
        public int? UserId { get; set; }
        public bool IsDefault { get; set; }
        public int? Columns { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual InputType InputType { get; set; }
        public virtual ICollection<InputTypeViewField> InputTypeViewField { get; set; }
    }
}
