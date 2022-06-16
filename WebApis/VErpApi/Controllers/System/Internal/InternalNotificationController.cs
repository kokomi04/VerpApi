using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalNotificationController : CrossServiceBaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ISubscriptionService _subscriptionService;

        public InternalNotificationController(INotificationService notificationService, ISubscriptionService subscriptionService)
        {
            _notificationService = notificationService;
            _subscriptionService = subscriptionService;
        }

        [HttpPost]
        [Route("")]
        public async Task<bool> AddNotification([FromBody] NotificationAdditionalModel model)
        {
            return await _notificationService.AddNotification(model);
        }

        [HttpPost]
        [Route("subscription")]
        public async Task<long> AddSubscription([FromBody] SubscriptionModel model)
        {
            return await _subscriptionService.AddSubscription(model);
        }

        [HttpPost]
        [Route("subscriptionToThePermissionPerson")]
        public async Task<bool> AddSubscriptionToThePermissionPerson([FromBody] SubscriptionToThePermissionPersonSimpleModel req)
        {
            return await _subscriptionService.AddSubscriptionToThePermissionPerson(req);
        }
    }
}