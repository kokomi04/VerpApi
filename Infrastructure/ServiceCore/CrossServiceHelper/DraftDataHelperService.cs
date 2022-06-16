using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Service;

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
            return await _httpCrossService.Deleted<bool>($"api/internal/InternalDraftData?objectTypeId={objectTypeId}&objectId={objectId}", new { });
        }
    }
}
