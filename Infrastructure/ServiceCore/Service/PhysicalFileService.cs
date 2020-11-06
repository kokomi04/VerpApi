using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface IPhysicalFileService
    {
        Task<bool> FileAssignToObject(long fileId, EnumObjectType objectTypeId, long objectId);
        Task<bool> DeleteFile(long fileId);
        Task<long> SaveSimpleFileInfo(EnumObjectType objectTypeId, SimpleFileInfo simpleFile);
        Task<SimpleFileInfo> GetSimpleFileInfo(long fileId);
    }

    public class PhysicalFileService : IPhysicalFileService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly ICurrentContextService _currentContext;

        public PhysicalFileService(IHttpCrossService httpCrossService, ILogger<IPhysicalFileService> logger, IOptionsSnapshot<AppSetting> appSetting, ICurrentContextService currentContext)
        {
            _httpCrossService = httpCrossService;
            _logger = logger;
            _appSetting = appSetting.Value;
            _currentContext = currentContext;
        }

        public async Task<bool> FileAssignToObject(long fileId, EnumObjectType objectTypeId, long objectId)
        {
            return await _httpCrossService.Put<bool>($"/api/internal/InternalFile/{fileId}/FileAssignToObject", new FileAssignToObjectInput
            {
                ObjectTypeId = objectTypeId,
                ObjectId = objectId
            });
        }

        public async Task<bool> DeleteFile(long fileId)
        {
            return await _httpCrossService.Detete<bool>($"/api/internal/InternalFile/{fileId}", new
            {

            });
        }

        public async Task<long> SaveSimpleFileInfo(EnumObjectType objectTypeId, SimpleFileInfo simpleFile)
        {
            return await _httpCrossService.Post<long>($"/api/internal/InternalFile/{objectTypeId}", simpleFile);
        }

        public async Task<SimpleFileInfo> GetSimpleFileInfo(long fileId)
        {
            return await _httpCrossService.Get<SimpleFileInfo>($"/api/internal/InternalFile/{fileId}");
        }
    }
}
