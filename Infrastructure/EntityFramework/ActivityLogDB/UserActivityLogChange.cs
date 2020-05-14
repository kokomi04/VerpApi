using System;
using System.Collections.Generic;

namespace ActivityLogDB
{
    public partial class UserActivityLogChange
    {
        public long UserActivityLogId { get; set; }
        public string ObjectChange { get; set; }
    }
}
