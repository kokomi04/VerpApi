using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class UserActivityLogChange
    {
        public long UserActivityLogId { get; set; }
        public string ObjectChange { get; set; }
    }
}
