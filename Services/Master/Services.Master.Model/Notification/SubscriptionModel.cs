using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using ActivityLogDB;

namespace VErp.Services.Master.Model.Notification
{
    public class SubscriptionModel: SubscriptionSimpleModel, IMapFrom<Subscription>
    {
        public long SubscriptionId { get; set; }
      
    }
}