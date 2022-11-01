using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.SignalR;
using static VErp.Commons.Constants.Caching.AuthorizeCachingTtlConstants;

namespace VErp.Infrastructure.ApiCore.BackgroundTasks
{
    public class LongTaskStatusService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IHubContext<BroadcastSignalRHub, IBroadcastHubClient> hubNotifyContext;
        private Timer _timer;
        private ILongTaskResourceInfo info = null;

        public LongTaskStatusService(ILogger<LongTaskStatusService> logger, IHubContext<BroadcastSignalRHub, IBroadcastHubClient> hubNotifyContext)
        {
            _logger = logger;
            this.hubNotifyContext = hubNotifyContext;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Long task status service running.");


            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }


        private async void DoWork(object state)
        {
            var currentInfo = LongTaskResourceLockFactory.GetCurrentProcess();
            if (currentInfo != info)
            {
                info = currentInfo;
                await hubNotifyContext.Clients.All.LongTaskStatus(info);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Long task status service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
