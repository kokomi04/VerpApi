using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using static VErp.Commons.Constants.Caching.AuthorizeCacheKeys;
using static VErp.Commons.Constants.Caching.AuthorizeCachingTtlConstants;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IAuthDataCacheService
    {
        Task<User> UserInfo(int userId);

        Task<IDictionary<int, List<ActionButtonActionType>>> ActionButtons();

        Task<IDictionary<Guid, ApiEndpoint>> ApiEndpoints();
    }


    public class AuthDataCacheService : IAuthDataCacheService
    {

        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICachingService _cachingService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AuthDataCacheService(
           IOptionsSnapshot<AppSetting> appSetting
            , ILogger<AuthDataCacheService> logger
            , ICachingService cachingService
            , IServiceScopeFactory serviceScopeFactory
       )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _cachingService = cachingService;
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task<User> UserInfo(int userId)
        {
            return await TryGetSet(UserInfoCacheKey(userId), async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var masterDBContext = scope.ServiceProvider.GetRequiredService<UnAuthorizeMasterDBContext>();

                    return await masterDBContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
                }

            });
        }

        public async Task<IDictionary<int, List<ActionButtonActionType>>> ActionButtons()
        {
            var t = AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = AUTHORIZED_CACHING_TIMEOUT;
            }
            return await _cachingService.TryGetSet(AUTH_TAG, ActionButtonsCacheKey(), t, async () =>
           {
               using (var scope = _serviceScopeFactory.CreateScope())
               {
                   var masterDBContext = scope.ServiceProvider.GetRequiredService<UnAuthorizeMasterDBContext>();

                   return (await (from m in masterDBContext.ActionButtonBillType
                                  join b in masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                                  select new ActionButtonActionType
                                  {
                                      ActionButtonId = m.ActionButtonId,
                                      BillTypeObjectTypeId = (EnumObjectType)b.BillTypeObjectTypeId,
                                      BillTypeObjectId = m.BillTypeObjectId,
                                      ActionType = b.ActionTypeId ?? (int)EnumActionType.View
                                  }
                            ).ToListAsync())
                            .GroupBy(m => m.ActionButtonId)
                            .ToDictionary(m => m.Key, m => m.ToList());

               }
           });
        }

        public async Task<IDictionary<Guid, ApiEndpoint>> ApiEndpoints()
        {
            var t = AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = AUTHORIZED_CACHING_TIMEOUT;
            }

            return await _cachingService.TryGetSet(AUTH_TAG, ApiEndpointsCacheKey(), t, async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var masterDBContext = scope.ServiceProvider.GetRequiredService<UnAuthorizeMasterDBContext>();

                    return (await masterDBContext.ApiEndpoint.AsNoTracking().ToListAsync()).ToDictionary(e => e.ApiEndpointId, e => e);
                }
            });

        }

        //private T TryGetSet<T>(string key, Func<T> queryData)
        //{
        //    return _cachingService.TryGetSet(key, AUTHORIZED_CACHING_TIMEOUT, queryData);
        //}

        private Task<T> TryGetSet<T>(string key, Func<Task<T>> queryData)
        {
            return _cachingService.TryGetSet(AUTH_TAG, key, AUTHORIZED_CACHING_TIMEOUT, queryData);
        }

        private Task<T> TryGetSetLong<T>(string key, Func<Task<T>> queryData)
        {
            return _cachingService.TryGetSet(AUTH_TAG, key, AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT, queryData);
        }
    }
}
