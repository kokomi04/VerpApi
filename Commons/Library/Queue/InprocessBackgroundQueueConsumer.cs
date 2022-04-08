using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VErp.Commons.Library.Queue
{

    internal class InprocessBackgroundQueueConsumer<IService, T> : BackgroundService
    {
        private readonly IService _service;
        private readonly ILogger _logger;
        private readonly string _queueName;
        private readonly IBackgroundTaskQueueStore _store;
        private readonly Func<IService, ProcessQueueMessage<T>, CancellationToken, Task> _func;

        public InprocessBackgroundQueueConsumer(IService service,
            ILoggerFactory loggerFactory,
            IBackgroundTaskQueueStore store,
            string queueName,
            Func<IService, ProcessQueueMessage<T>, CancellationToken, Task> func)
        {
            _service = service;
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
                    await _func(_service, msg, cancellationToken);
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
