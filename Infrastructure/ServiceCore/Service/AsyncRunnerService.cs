using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IAsyncRunnerService
    {
        void RunAsync<T>(Expression<Func<T, Task>> action);
    }
    public class AsyncRunnerService: IAsyncRunnerService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AsyncRunnerService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void RunAsync<T>(Expression<Func<T, Task>> action)
        {
            Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var obj = scope.ServiceProvider.GetService<T>();
                    var fn = action.Compile();
                    await fn.Invoke(obj);
                }
            });
            
        }
    }
}
