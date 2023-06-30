using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;

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

        public HttpGuideCrossService(IHttpClientFactoryService httpClient, ILogger<HttpCrossService> logger, IOptionsSnapshot<AppSetting> appSetting, MasterDBContext masterDBContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _masterDBContext = masterDBContext;
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
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GetFullUriPath(string relativeUrl)
        {
            var guideApiEndpoint = await _masterDBContext.Config.Where(x => x.ConfigName == GuideConstants.GuideApiEndpoint).FirstOrDefaultAsync();

            return $"{guideApiEndpoint.Value.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
        private async void SetContextHeaders(HttpRequestMessage request)
        {
            var guideApiKey = await _masterDBContext.Config.Where(x=>x.ConfigName == GuideConstants.GuideApiKey).FirstOrDefaultAsync();

            request.Headers.TryAddWithoutValidation(Headers.GuideServiceKey, guideApiKey.Value);
        }

    }
}
