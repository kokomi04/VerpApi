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
    public interface IPhysicalFileService
    {
        Task<bool> FileAssignToObject(long fileId, EnumObjectType objectTypeId, long objectId);
        Task<bool> DeleteFile(long fileId);
    }

    public class PhysicalFileService : IPhysicalFileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public PhysicalFileService(HttpClient httpClient, ILogger<IPhysicalFileService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
        }

        public async Task<bool> FileAssignToObject(long fileId, EnumObjectType objectTypeId, long objectId)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/api/internal/InternalFile/{fileId}/FileAssignToObject";

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put,
                    Content = new StringContent(new FileAssignToObjectInput
                    {
                        ObjectTypeId = objectTypeId,
                        ObjectId = objectId
                    }.JsonSerialize(), Encoding.UTF8, "application/json"),
                };

                request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);

                var data = await _httpClient.SendAsync(request);

                return data.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PhysicalFileService:FileAssignToObject");
                return false;
            }
        }

        public async Task<bool> DeleteFile(long fileId)
        {
            try
            {
                var uri = $"{_appSetting.ServiceUrls.ApiService.Endpoint.TrimEnd('/')}/api/internal/InternalFile/{fileId}";

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Delete,
                    Content = new StringContent(new object
                    {
                        
                    }.JsonSerialize(), Encoding.UTF8, "application/json"),
                };

                request.Headers.TryAddWithoutValidation(Headers.CrossServiceKey, _appSetting?.Configuration?.InternalCrossServiceKey);

                var data = await _httpClient.SendAsync(request);

                return data.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PhysicalFileService:DeleteFile");
                return false;
            }
        }
    }
}
