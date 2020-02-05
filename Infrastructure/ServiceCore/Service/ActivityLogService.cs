using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;

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

        public ActivityLogService(HttpClient httpClient, ILogger logger, IOptionsSnapshot<AppSetting> appSetting)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
        }

        public async Task<bool> CreateLog(EnumObjectType objectTypeId, long objectId, string message, string jsonData)
        {
            var uri = $"{_appSetting.ServiceUrls.ApiService}api/internal/InternalActivityLog/Log";

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Post,
                Content = new StringContent(new
                {
                    objectTypeId,
                    objectId,
                    message,
                    data = jsonData
                }.JsonSerialize(), Encoding.UTF8, "application/json"),
            };

            var data = await _httpClient.SendAsync(request);

            return data.IsSuccessStatusCode;
        }
    }
}
