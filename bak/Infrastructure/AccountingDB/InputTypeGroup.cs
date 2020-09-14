using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputTypeGroup
    {
        public InputTypeGroup()
        {
            InputType = new HashSet<InputType>();
        }

        public int InputTypeGroupId { get; set; }
        public string InputTypeGroupName { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<InputType> InputType { get; set; }
    }
}
