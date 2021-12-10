using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IList<NotificationModel>> GetListByUserId([FromQuery] int userId)
        {
            return await _notificationService.GetListByUserId(userId);
        }

        [HttpPut]
        [Route("markerRead")]
        public async Task<bool> MarkerReadNotification([FromBody] long[] lsNotificationId)
        {
            return await _notificationService.MarkerReadNotification(lsNotificationId);
        }

        [HttpGet]
        [Route("subscription")]
        public async Task<IList<SubscriptionModel>> GetListSubscriptionByUserId([FromQuery] int userId)
        {
            return await _subscriptionService.GetListByUserId(userId);
        }

        [HttpDelete]
        [Route("subscription/{subscriptionId}")]
        public async Task<bool> UnSubscription([FromRoute] long subscriptionId)
        {
            return await _subscriptionService.UnSubscription(subscriptionId);
        }


    }
}