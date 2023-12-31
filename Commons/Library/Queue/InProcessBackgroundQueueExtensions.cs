﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Library.Queue.Interfaces;

namespace VErp.Commons.Library.Queue
{
    public static class InProcessBackgroundQueueExtensions
    {
        public static void AddInProcessBackgroundQueuePublisher(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundTaskQueueStore, BackgroundTaskQueueStore>();
            services.TryAddSingleton<IMessageQueuePublisher>(s => s.GetRequiredService<IBackgroundTaskQueueStore>());
        }

        public static void AddInProcessBackgroundConsummer<IService, T>(this IServiceCollection services, string queueName, Func<IService, ProcessQueueMessage<T>, CancellationToken, Task> _func)
        {
            services.AddSingleton<IHostedService>(s =>
            {
                var serviceScopeFactory = s.GetRequiredService<IServiceScopeFactory>();
                var store = s.GetRequiredService<IBackgroundTaskQueueStore>();
                var loggerFactory = s.GetRequiredService<ILoggerFactory>();

                return new InprocessBackgroundQueueConsumer<IService, T>(serviceScopeFactory, loggerFactory, store, queueName, _func);
            });
        }
    }
}
