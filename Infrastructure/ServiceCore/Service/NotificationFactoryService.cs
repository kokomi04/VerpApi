using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface INotificationFactoryService
    {
        Task<long> AddSubscription(SubscriptionSimpleModel model);
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
    }
}