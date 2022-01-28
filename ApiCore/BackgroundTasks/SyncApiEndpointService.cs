using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Commons.Constants.Caching.AuthorizeCachingTtlConstants;

namespace VErp.Infrastructure.ApiCore.BackgroundTasks
{
    public class SyncApiEndpointService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly IAuthDataCacheService _authDataService;

        public SyncApiEndpointService(ILogger<SyncApiEndpointService> logger, IAuthDataCacheService authDataService)
        {
            _logger = logger;
            _authDataService = authDataService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sync api endpoint service running.");
            var t = AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = AUTHORIZED_CACHING_TIMEOUT;
            }

            _timer = new Timer(DoWork, null, TimeSpan.Zero, t);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            await _authDataService.ApiEndpoints();
            // _logger.LogInformation("Sync api endpoint service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sync api endpoint service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
