using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/notification")]
    public class NotificationController: VErpBaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ISubscriptionService _subscriptionService;

        public NotificationController(INotificationService notificationService, ISubscriptionService subscriptionService)
        {
            _notificationService = notificationService;
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<IList<NotificationModel>> GetListByUserId()
        {
            return await _notificationService.GetListByUserId();
        }
        
        [HttpGet]
        [Route("numberOfUnRead")]
        [GlobalApi]
        public async Task<long> GetCountNotification()
        {
            return await _notificationService.GetCountNotification();
        }

        [HttpPut]
        [Route("markerRead")]
        [GlobalApi]
        public async Task<bool> MarkerReadNotification([FromBody] long[] lsNotificationId, [FromQuery] bool mark)
        {
            return await _notificationService.MarkerReadNotification(lsNotificationId, mark);
        }

        [HttpGet]
        [Route("subscription")]
        [GlobalApi]
        public async Task<IList<SubscriptionModel>> GetListSubscriptionByUserId()
        {
            return await _subscriptionService.GetListByUserId();
        }

        [HttpGet]
        [Route("subscription/check")]
        [GlobalApi]
        public async Task<bool> CheckSubscriptionByUserId([FromQuery] int objectTypeId, [FromQuery] int objectId, [FromQuery] int? billTypeId)
        {
            return await _subscriptionService.CheckSubscription(new CheckSubscriptionSimpleModel 
            {
                BillTypeId = billTypeId,
                ObjectId = objectId,
                ObjectTypeId = objectTypeId
            });
        }

        [HttpPut]
        [Route("subscription/marker")]
        [GlobalApi]
        public async Task<bool> UnSubscriptionByUserId([FromQuery] int objectTypeId, [FromQuery] int objectId, [FromQuery] int? billTypeId, [FromQuery] bool marker)
        {
            return await _subscriptionService.MarkerSubscription(new CheckSubscriptionSimpleModel
            {
                BillTypeId = billTypeId,
                ObjectId = objectId,
                ObjectTypeId = objectTypeId
            }, marker);
        }

        [HttpPost]
        [Route("subscription")]
        [GlobalApi]
        public async Task<long> AddSubscription([FromBody] SubscriptionModel model)
        {
            return await _subscriptionService.AddSubscription(model);
        }

        [HttpDelete]
        [Route("subscription/{subscriptionId}")]
        [GlobalApi]
        public async Task<bool> UnSubscription([FromRoute] long subscriptionId)
        {
            return await _subscriptionService.UnSubscription(subscriptionId);
        }


    }
}