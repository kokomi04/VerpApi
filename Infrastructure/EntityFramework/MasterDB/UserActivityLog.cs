using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class UserActivityLog
    {
        public long UserActivityLogId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string Message { get; set; }
        public int? ActionId { get; set; }
    }
}
