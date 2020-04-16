using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputType : BaseEntity
    {
        public InputType()
        {
            InputAreas = new HashSet<InputArea>();
        }

        public int InputTypeId { get; set; }
        public string Title { get; set; }
        public string InputTypeCode { get; set; }

        public virtual ICollection<InputArea> InputAreas { get; set; }

    }
}
