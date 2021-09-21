using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Action
    {
        public Action()
        {
            ApiEndpoint = new HashSet<ApiEndpoint>();
        }

        public int ActionId { get; set; }
        public string ActionName { get; set; }

        public virtual ICollection<ApiEndpoint> ApiEndpoint { get; set; }
    }
}
