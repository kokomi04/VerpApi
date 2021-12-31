using System;
using System.Collections.Generic;

#nullable disable

namespace ActivityLogDB
{
    public partial class PushSubscription
    {
        public long PushSubscriptionId { get; set; }
        public int UserId { get; set; }
        public string Endpoint { get; set; }
        public long? ExpirationTime { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
