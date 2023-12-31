﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class Method
{
    public int MethodId { get; set; }

    public string MethodName { get; set; }

    public virtual ICollection<ApiEndpoint> ApiEndpoint { get; set; } = new List<ApiEndpoint>();
}
