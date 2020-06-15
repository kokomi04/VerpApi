using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        Task<T> Get<T>(string relativeUrl);
    }

    public class HttpCrossService : IHttpCrossService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public HttpCrossService(HttpClient httpClient, ILogger<ActivityLogService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
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

                request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);
                request.Headers.TryAddWithoutValidation(Headers.UserId, _currentContext.UserId.ToString());
                request.Headers.TryAddWithoutValidation(Headers.Action, _currentContext.Action.ToString());

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    _logger.LogError($"HttpCrossService:Post {uri} {{0}} Error {data.StatusCode} {{1}}", body, response);
                    throw new BadRequestException(GeneralCode.InternalError, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Post");
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }

        public async Task<T> Get<T>(string relativeUrl)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
                
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };

                request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);
                request.Headers.TryAddWithoutValidation(Headers.UserId, _currentContext.UserId.ToString());
                request.Headers.TryAddWithoutValidation(Headers.Action, _currentContext.Action.ToString());

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    _logger.LogError($"HttpCrossService:Get {uri} Error {data.StatusCode} {{0}}", response);
                    throw new BadRequestException(GeneralCode.InternalError, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpCrossService:Get");
                throw new BadRequestException(GeneralCode.InternalError, ex.Message);
            }
        }
    }
}
