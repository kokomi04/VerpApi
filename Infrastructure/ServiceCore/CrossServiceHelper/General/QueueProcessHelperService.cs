using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.General
{
    public interface IQueueProcessHelperService
    {
        Task<bool> EnqueueAsync<T>(string queueName, T data);
    }


    public class QueueProcessHelperService : IQueueProcessHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public QueueProcessHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<bool> EnqueueAsync<T>(string queueName, T data)
        {

            return await _httpCrossService.Post<bool>("api/internal/InternalQueueProcess/Enqueue", new { queueName, data = data.JsonSerialize() });
        }
    }
}
