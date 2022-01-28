
using VErp.Commons.GlobalObject;
using ActivityLogDB;

namespace VErp.Services.Master.Model.WebPush
{
    public class PushSubscriptionModel: IMapFrom<PushSubscription>
    {
        public long PushSubscriptionId { get; set; }
        public int UserId { get; set; }
        public string Endpoint { get; set; }
        public long? ExpirationTime { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
    }
}