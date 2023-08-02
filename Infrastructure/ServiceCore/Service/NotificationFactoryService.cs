using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.System;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface INotificationFactoryService
    {
        Task<long> AddSubscription(SubscriptionSimpleModel model);
        Task<bool> AddSubscriptionToThePermissionPerson(SubscriptionToThePermissionPersonSimpleModel model);
    }

    public class NotificationFactoryService : INotificationFactoryService
    {
        private readonly IHttpCrossService _httpCrossService;

        public NotificationFactoryService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<long> AddSubscription(SubscriptionSimpleModel model)
        {
            return await _httpCrossService.Post<long>($"/api/internal/InternalNotification/subscription", model);
        }

        public async Task<bool> AddSubscriptionToThePermissionPerson(SubscriptionToThePermissionPersonSimpleModel model)
        {
            return await _httpCrossService.Post<bool>($"/api/internal/InternalNotification/subscriptionToThePermissionPerson", model);
        }
    }
}