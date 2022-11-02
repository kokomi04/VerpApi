using DocumentFormat.OpenXml.Spreadsheet;
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
using VErp.Commons.GlobalObject;

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


            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            //Task.Factory.StartNew(async () =>
            //{
            //    var creationInfo = new LongTaskCreationInfo("", 2, "admin", "admin", DateTime.UtcNow.GetUnix());

            //    using (var t = await LongTaskResourceLockFactory.Accquire("Test proce", creationInfo))
            //    {
            //        t.SetCurrentStep("step 1", 100);
            //        for (var i = 0; i < 30; i++)
            //        {
            //            if (i % 100 == 0)
            //            {
            //                t.SetCurrentStep("Step " + i / 100, 100);
            //            }
            //            await Task.Delay(1000);
            //            t.IncProcessedRows();
            //        }
            //    }
            //});
            return Task.CompletedTask;
        }

        private DateTime lastSent = DateTime.UtcNow;

        private async void DoWork(object state)
        {
            var currentInfo = LongTaskResourceLockFactory.GetCurrentProcess();
            if (currentInfo != info || DateTime.Now.Subtract(lastSent).TotalSeconds > 10)
            {
                if (DateTime.Now.Subtract(lastSent).TotalSeconds > 10)
                {
                    LongTaskResourceLockFactory.UnWatchStatus();
                }
                lastSent = DateTime.UtcNow;
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
