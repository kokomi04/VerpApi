using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalNotificationController: CrossServiceBaseController
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
        public async Task<bool> AddNotification(NotificationAdditionalModel model)
        {
            return await _notificationService.AddNotification(model);
        }

        [HttpPost]
        [Route("subscription")]
        public async Task<long> AddSubscription(SubscriptionModel model)
        {
            return await _subscriptionService.AddSubscription(model);
        }
    }
}