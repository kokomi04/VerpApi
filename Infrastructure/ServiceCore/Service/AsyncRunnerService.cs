using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IAsyncRunnerService
    {
        void RunAsync<T>(Expression<Func<T, Task>> action);
    }
    public class AsyncRunnerService : IAsyncRunnerService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICurrentContextService _currentContext;
        private readonly ILogger _logger;
        public AsyncRunnerService(IServiceScopeFactory serviceScopeFactory, ICurrentContextService currentContext, ILogger<AsyncRunnerService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _currentContext = currentContext;
            _logger = logger;
        }

        public void RunAsync<T>(Expression<Func<T, Task>> action)
        {
            try
            {
                var userId = _currentContext.UserId;
                var actionId = _currentContext.Action;
                var stockIds = _currentContext.StockIds;
                var roleInfo = _currentContext.RoleInfo;
                var subsidiaryId = _currentContext.SubsidiaryId;
                var timeZone = _currentContext.TimeZoneOffset;
                var language = _currentContext.Language;
                var ipAddress = _currentContext.IpAddress;
                var domain = _currentContext.Domain;
                Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var currentContextFactory = scope.ServiceProvider.GetRequiredService<ICurrentContextFactory>();
                            currentContextFactory.SetCurrentContext(new ScopeCurrentContextService(userId, actionId, roleInfo, stockIds, subsidiaryId, timeZone, language, ipAddress, domain));
                            var obj = scope.ServiceProvider.GetService<T>();
                            var fn = action.Compile();
                            await fn.Invoke(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "RunAsyncTask");
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IAsyncRunnerService:RunAsync");
                throw;
            }

        }
    }
}
