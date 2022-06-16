using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IHttpCrossService
    {
        Task<T> Post<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Put<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Deleted<T>(string relativeUrl, object postData, object queries = null);
        Task<T> Get<T>(string relativeUrl, object queries = null);
    }

    public class HttpCrossService : IHttpCrossService
    {
        private readonly IHttpClientFactoryService _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public HttpCrossService(IHttpClientFactoryService httpClient, ILogger<HttpCrossService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
        }

        public async Task<T> Post<T>(string relativeUrl, object postData, object queries = null)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
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
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
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
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                var body = postData.JsonSerialize();


                return await _httpClient.Deleted<T>(uri, postData, request => SetContextHeaders(request), queries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Put");
                throw;
            }
        }

        public async Task<T> Get<T>(string relativeUrl, object queries = null)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";

                return await _httpClient.Get<T>(uri, queries, request => SetContextHeaders(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Get");
                throw;
            }
        }

        private void SetContextHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);
            request.Headers.TryAddWithoutValidation(Headers.UserId, _currentContext.UserId.ToString());
            request.Headers.TryAddWithoutValidation(Headers.Action, ((int)_currentContext.Action).ToString());
            request.Headers.TryAddWithoutValidation(Headers.TimeZoneOffset, _currentContext.TimeZoneOffset.ToString());
            request.Headers.TryAddWithoutValidation(Headers.SubsidiaryId, _currentContext.SubsidiaryId.ToString());
        }

    }
}
