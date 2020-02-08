﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
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

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new StringContent(new ActivityInput
                    {
                        UserId = _currentContext.UserId,
                        ActionId = _currentContext.Action,
                        ObjectTypeId = objectTypeId,
                        ObjectId = objectId,
                        Message = message,
                        Data = jsonData
                    }.JsonSerialize(), Encoding.UTF8, "application/json"),
                };

                var data = await _httpClient.SendAsync(request);

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
