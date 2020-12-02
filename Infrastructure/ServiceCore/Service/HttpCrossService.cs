using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VErp.Commons.Constants;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IHttpCrossService
    {
        Task<T> Post<T>(string relativeUrl, object postData);
        Task<T> Put<T>(string relativeUrl, object postData);
        Task<T> Detete<T>(string relativeUrl, object postData);
        Task<T> Get<T>(string relativeUrl, IDictionary<string, object> queries = null);
        Task<T> Get<T>(string relativeUrl, object queries);
    }

    public class HttpCrossService : IHttpCrossService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public HttpCrossService(HttpClient httpClient, ILogger<HttpCrossService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
        }

        public async Task<T> Post<T>(string relativeUrl, object postData)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                SetContextHeaders(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("POST", uri, postData, data, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Post");
                throw;
            }
        }


        public async Task<T> Put<T>(string relativeUrl, object postData)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                SetContextHeaders(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("PUT", uri, postData, data, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Put");
                throw;
            }
        }


        public async Task<T> Detete<T>(string relativeUrl, object postData)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Delete,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                SetContextHeaders(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("DELETE", uri, postData, data, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Put");
                throw;
            }
        }

        public async Task<T> Get<T>(string relativeUrl, IDictionary<string, object> queries = null)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                var queryData = new Dictionary<string, string>();
                if (queries != null && queries.Count > 0)
                {
                    foreach (var (k, v) in queries)
                    {
                        if (!v.IsNullObject())
                        {
                            if (v.GetType().IsEnum)
                            {
                                queryData.Add(k, ((int)v).ToString());
                            }
                            else
                            {
                                queryData.Add(k, v.ToString());
                            }
                        }
                    }
                }
                if (queryData.Count > 0)
                {
                    uri = QueryHelpers.AddQueryString(uri, queryData);
                }

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };
                SetContextHeaders(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("GET", uri, null, data, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Get");
                throw;
            }
        }

        public async Task<T> Get<T>(string relativeUrl, object queries)
        {
            var dicQueries = new Dictionary<string, object>();
            if (!queries.IsNullObject())
            {
                var props = queries.GetType().GetProperties();                
                foreach (var prop in props)
                {
                    dicQueries.Add(prop.Name, prop.GetValue(queries));
                }
            }
            return await Get<T>(relativeUrl, dicQueries);
        }

        private void SetContextHeaders(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);
            request.Headers.TryAddWithoutValidation(Headers.UserId, _currentContext.UserId.ToString());
            request.Headers.TryAddWithoutValidation(Headers.Action, ((int)_currentContext.Action).ToString());
            request.Headers.TryAddWithoutValidation(Headers.TimeZoneOffset, _currentContext.TimeZoneOffset.ToString());
            request.Headers.TryAddWithoutValidation(Headers.SubsidiaryId, _currentContext.SubsidiaryId.ToString());
        }

        private void ThrowErrorResponse(string method, string uri, object body, HttpResponseMessage responseMessage, string response)
        {
            ApiErrorResponse result = null;
            try
            {
                result = response.JsonDeserialize<ApiErrorResponse>();
            }
            catch (Exception)
            {

            }

            if (result != null)
            {
                _logger.LogError($"HttpCrossService:{method} {uri} {{0}} Warning {responseMessage.StatusCode} {{1}}", body, response);

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, result.Message);
                }
                else
                {
                    throw new Exception(result.Message);
                }
            }

            _logger.LogError($"HttpCrossService:{method} {uri} {{0}} Error {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} {{1}}", body, response);

            throw new Exception($"{(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} {response}");
        }



    }
}
