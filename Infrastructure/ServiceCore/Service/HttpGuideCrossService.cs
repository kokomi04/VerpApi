using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using static VErp.Commons.Constants.Caching.ConfigCacheKeys;
using static VErp.Commons.Constants.Caching.ConfigCachingTtlConstants;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IHttpGuideCrossService
    {
        Task<T> Post<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Put<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Deleted<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Get<T>(string relativeUrl, object queries = null);
    }

    public class HttpGuideCrossService : IHttpGuideCrossService
    {
        private readonly IHttpClientFactoryService _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly MasterDBContext _masterDBContext;
        private readonly ICachingService _cachingService;

        public HttpGuideCrossService(IHttpClientFactoryService httpClient, ILogger<HttpCrossService> logger, IOptionsSnapshot<AppSetting> appSetting, MasterDBContext masterDBContext, ICachingService cachingService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _masterDBContext = masterDBContext;
            _cachingService = cachingService;
        }

        public async Task<T> Post<T>(string relativeUrl, object postData, object queries = null)
        {
            try
            {
                var uri = await GetFullUriPath(relativeUrl);
                var body = postData.JsonSerialize();

                return await _httpClient.Post<T>(uri, postData, request => SetContextHeaders(request), null, null, queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Post");
                throw;
            }
        }


        public async Task<T> Put<T>(string relativeUrl, object postData, object queries = null)
        {
            try
            {
                var uri = await GetFullUriPath(relativeUrl);

                var body = postData.JsonSerialize();

                return await _httpClient.Put<T>(uri, postData, request => SetContextHeaders(request), null, queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Put");
                throw;
            }
        }


        public async Task<T> Deleted<T>(string relativeUrl, object postData, object queries = null)
        {
            try
            {
                var uri = await GetFullUriPath(relativeUrl);
                var body = postData.JsonSerialize();


                return await _httpClient.Deleted<T>(uri, postData, request => SetContextHeaders(request), queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Deleted");
                throw;
            }
        }

        public async Task<T> Get<T>(string relativeUrl, object queries = null)
        {
            try
            {
                var uri = await GetFullUriPath(relativeUrl);

                return await _httpClient.Get<T>(uri, queries, request => SetContextHeaders(request));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> GetFullUriPath(string relativeUrl)
        {
            var t = CONFIG_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = CONFIG_CACHING_TIMEOUT;
            }

            var guideApiEndpoint = await _cachingService.TryGetSet(CONFIG_TAG, ConfigCacheKey(GuideConstants.GuideApiEndpoint), t, async () =>
            {
                return await _masterDBContext.Config
                    .Where(x => x.ConfigName == GuideConstants.GuideApiEndpoint)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync();
            });

            return $"{guideApiEndpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
        private async void SetContextHeaders(HttpRequestMessage request)
        {
            var t = CONFIG_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = CONFIG_CACHING_TIMEOUT;
            }

            var guideApiKey = await _cachingService.TryGetSet(CONFIG_TAG, ConfigCacheKey(GuideConstants.GuideApiKey), t, async () =>
            {
                return await _masterDBContext.Config
                    .Where(x => x.ConfigName == GuideConstants.GuideApiKey)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync();
            });

            request.Headers.TryAddWithoutValidation(Headers.GuideServiceKey, guideApiKey);
        }

    }
}
