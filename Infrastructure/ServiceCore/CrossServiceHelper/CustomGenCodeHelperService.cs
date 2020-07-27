using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICustomGenCodeHelperService
    {
        Task<bool> MapObjectCustomGenCode(EnumObjectType objectTypeId, Dictionary<int, int> data);

        Task<CustomGenCodeOutputModelOut> CurrentConfig(EnumObjectType objectTypeId, int objectId);
        Task<CustomCodeModelOutput> GenerateCode(int customGenCodeId, int lastValue);
        Task<bool> ConfirmCode(EnumObjectType objectTypeId, int objectId);
    }

    public class CustomGenCodeHelperService : ICustomGenCodeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public CustomGenCodeHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }
        public async Task<bool> MapObjectCustomGenCode(EnumObjectType objectTypeId, Dictionary<int, int> data)
        {
            return await _httpCrossService.Post<bool>($"api/internal/InternalCustomGenCode/{(int)objectTypeId}/multiconfigs", data);
        }

        public async Task<CustomGenCodeOutputModelOut> CurrentConfig(EnumObjectType objectTypeId, int objectId)
        {
            return await _httpCrossService.Get<CustomGenCodeOutputModelOut>($"api/internal/InternalCustomGenCode/currentConfig?objectTypeId={(int)objectTypeId}&objectId={objectId}");
        }

        public async Task<CustomCodeModelOutput> GenerateCode(int customGenCodeId, int lastValue)
        {
            return await _httpCrossService.Get<CustomCodeModelOutput>($"api/internal/InternalCustomGenCode/generateCode?customGenCodeId={customGenCodeId}&lastValue={lastValue}");
        }

        public async Task<bool> ConfirmCode(EnumObjectType objectTypeId, int objectId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalCustomGenCode/confirmCode?objectTypeId={(int)objectTypeId}&objectId={objectId}", null);
        }

    }
}
