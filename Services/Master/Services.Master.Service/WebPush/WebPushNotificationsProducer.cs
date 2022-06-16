using ActivityLogDB;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.SignalR;
using VErp.Services.Master.Model.WebPush;

namespace VErp.Services.Master.Service.Webpush
{
    public class WebPushNotificationsProducer : IHostedService, IDisposable
    {
        private const int NOTIFICATION_FREQUENCY = 60000;

        private Timer _timer;

        private readonly ActivityLogDBContext _activityLogContext;
        private readonly PushServiceClient _pushClient;
        private readonly AppSetting _appSetting;
        private readonly IPrincipalBroadcasterService _principalBroadcaster;

        public WebPushNotificationsProducer(IOptions<AppSetting> appSetting,
                                            PushServiceClient pushClient,
                                            IPrincipalBroadcasterService principalBroadcaster, UnAuthorizActivityLogDBContext activityLogContext)
        {
            _pushClient = pushClient;
            _appSetting = appSetting.Value;
            _pushClient.DefaultAuthentication = new VapidAuthentication(_appSetting.WebPush.PublicKey, _appSetting.WebPush.PrivateKey);
            _principalBroadcaster = principalBroadcaster;
            _activityLogContext = activityLogContext;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMilliseconds(NOTIFICATION_FREQUENCY));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            if (!_appSetting.WebPush.EnableWorker) return;

            PushMessage notification = new AngularPushNotification
            {
                Title = "VERP DEMO WEBPUSH",
                Body = $"ABC",
            }.ToPushMessage();

            var subs = await _activityLogContext.PushSubscription.AsNoTracking().ToListAsync();

            foreach (var subscription in subs)
            {
                if (!_principalBroadcaster.IsUserConnected(subscription.UserId.ToString()))
                {
                    var keys = new Dictionary<string, string>();
                    keys.Add("auth", subscription.Auth);
                    keys.Add("p256dh", subscription.P256dh);

                    // Fire-and-forget 
                    await _pushClient.RequestPushMessageDeliveryAsync(new Lib.Net.Http.WebPush.PushSubscription()
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = keys,
                    }, notification);
                }

            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}