using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IActivityLogService
    {
        Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public ActivityLogService(HttpClient httpClient, ILogger<ActivityLogService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
        }

        public async Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/api/internal/InternalActivityLog/Log";

                var body = new ActivityInput
                {
                    UserId = _currentContext.UserId,
                    ActionId = _currentContext.Action,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId,
                    Message = message,
                    Data = jsonData
                }.JsonSerialize();

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);

                var data = await _httpClient.SendAsync(request);

                if (!data.IsSuccessStatusCode)
                {
                    var response = await data.Content.ReadAsStringAsync();
                    _logger.LogError($"CreateLog {uri} {{0}} Error {data.StatusCode} {{1}}", body, response);
                }

                return data.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActivityLogService:CreateLog");
                return false;
            }
        }
    }
}
