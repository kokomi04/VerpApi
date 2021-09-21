using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ActionType
    {
        public int ActionTypeId { get; set; }
        public string ActionTypeName { get; set; }
        public string ActionTitle { get; set; }
        public bool IsEditable { get; set; }
    }
}
