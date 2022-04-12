using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VErp.Commons.Library.Queue.Interfaces;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Queue
{
    internal interface IBackgroundTaskQueueStore : IMessageQueuePublisher
    {
        Task<ProcessQueueMessage<T>> DequeueAsync<T>(string queueName, CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueueStore : IBackgroundTaskQueueStore
    {
        private class QueueStore
        {
            public ConcurrentQueue<ProcessQueueMessage<string>> _workItems = new ConcurrentQueue<ProcessQueueMessage<string>>();
            public SemaphoreSlim _signal = new SemaphoreSlim(0);
        }

        private ConcurrentDictionary<string, QueueStore> _queueStore = new ConcurrentDictionary<string, QueueStore>();

        public Task EnqueueAsync(ICurrentContextService context, string queueName, string data, int userId)
        {

            var queueStore = _queueStore.GetOrAdd(queueName, new QueueStore());

            queueStore._workItems.Enqueue(new ProcessQueueMessage<string>() { Context = context, QueueName = queueName, Data = data, CreatedByUserId = userId, CreatedDatetimeUtc = DateTime.UtcNow });
            queueStore._signal.Release();

            return Task.CompletedTask;
        }

        public async Task<ProcessQueueMessage<T>> DequeueAsync<T>(string queueName, CancellationToken cancellationToken)
        {
            var queueStore = _queueStore.GetOrAdd(queueName, new QueueStore());

            await queueStore._signal.WaitAsync(cancellationToken);
            queueStore._workItems.TryDequeue(out var item);

            var msg = new ProcessQueueMessage<T>()
            {
                QueueName = item.QueueName,
                CreatedByUserId = item.CreatedByUserId,
                CreatedDatetimeUtc = item.CreatedDatetimeUtc,
                Data = item.Data.JsonDeserialize<T>()
            };
            return msg;
        }
    }

}
