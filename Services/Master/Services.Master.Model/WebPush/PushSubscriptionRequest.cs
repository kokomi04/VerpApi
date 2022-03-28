
using Lib.Net.Http.WebPush;

namespace VErp.Services.Master.Model.WebPush
{
    public class PushSubscriptionRequest: PushSubscription
    {
        public long? ExpirationTime { get; set; }
    }
}