using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;
using System.Data;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IDraftDataHelperService
    {
        Task<bool> DeleteDraftData(int objectTypeId, long objectId);
    }
    public class DraftDataHelperService : IDraftDataHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public DraftDataHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> DeleteDraftData(int objectTypeId, long objectId)
        {
            return await _httpCrossService.Detete<bool>($"api/internal/InternalDraftData?objectTypeId={objectTypeId}&objectId={objectId}", new { });
        }
    }
}
