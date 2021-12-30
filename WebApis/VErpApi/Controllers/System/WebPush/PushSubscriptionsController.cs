using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.WebPush;
using VErp.Services.Master.Service.Webpush;

namespace VErpApi.Controllers.System
{
    [Route("api/pushSubscription")]
    public class PushSubscriptionsController : VErpBaseController
    {
        private readonly IPushSubscriptionsService _pushSubscriptionService;

        public PushSubscriptionsController(IPushSubscriptionsService pushSubscriptionService)
        {
            _pushSubscriptionService = pushSubscriptionService;
        }

        [HttpPost]
        [Route("")]
        public async Task<bool> Subscribe([FromBody]PushSubscriptionRequest subscription)
        {
            return await _pushSubscriptionService.Subscribe(subscription);
        }

        [HttpDelete]
        [Route("")]
        public async Task<bool> UnSubscribe([FromQuery]string endpoint)
        {
            return  await _pushSubscriptionService.UnSubscribe(endpoint);
        }

        [HttpGet]
        [Route("publicKey")]
        public async Task<string> PublicKey()
        {
            return await _pushSubscriptionService.GetPublicKey();
        }
    }
}