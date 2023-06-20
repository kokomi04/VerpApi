using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class Action
{
    public int ActionId { get; set; }

    public string ActionName { get; set; }

    public virtual ICollection<ApiEndpoint> ApiEndpoint { get; set; } = new List<ApiEndpoint>();
}
