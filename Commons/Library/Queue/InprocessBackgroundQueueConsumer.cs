using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Queue
{

    internal class InprocessBackgroundQueueConsumer<IService, T> : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly string _queueName;
        private readonly IBackgroundTaskQueueStore _store;
        private readonly Func<IService, ProcessQueueMessage<T>, CancellationToken, Task> _func;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InprocessBackgroundQueueConsumer(
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory,
            IBackgroundTaskQueueStore store,
            string queueName,
            Func<IService, ProcessQueueMessage<T>, CancellationToken, Task> func
            )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<InprocessBackgroundQueueConsumer<IService, T>>();
            _queueName = queueName;
            _store = store;
            _func = func;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Queued {_queueName} Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var msg = await _store.DequeueAsync<T>(_queueName, cancellationToken);

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var currentContextFactory = scope.ServiceProvider.GetRequiredService<ICurrentContextFactory>();
                        var newContext = new ScopeCurrentContextService(msg.Context);
                        currentContextFactory.SetCurrentContext(newContext);
                        var service = scope.ServiceProvider.GetService<IService>();
                        await _func(service, msg, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred executing {0}.", msg.JsonSerialize());
                }
            }

            _logger.LogInformation($"Queued {_queueName} Hosted Service is stopping.");
        }
    }
}
