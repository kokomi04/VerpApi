using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Notification
    {
        public long NotificationId { get; set; }
        public int UserId { get; set; }
        public long UserActivityLogId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime? ReadDateTimeUtc { get; set; }
        public bool IsRead { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
