using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/notification")]
    public class NotificationController
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
        public async Task<IList<NotificationModel>> GetListByUserId([FromQuery] int userId)
        {
            return await _notificationService.GetListByUserId(userId);
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
        public async Task<IList<SubscriptionModel>> GetListSubscriptionByUserId([FromQuery] int userId)
        {
            return await _subscriptionService.GetListByUserId(userId);
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