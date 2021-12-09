using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Notification
{
    public class SubscriptionModel: SubscriptionSimpleModel, IMapFrom<Subscription>
    {
        public long SubscriptionId { get; set; }
      
    }
}