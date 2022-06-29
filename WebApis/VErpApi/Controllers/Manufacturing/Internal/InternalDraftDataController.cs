using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.DraftData;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalDraftDataController : CrossServiceBaseController
    {
        private readonly IDraftDataService _draftDataService;
        public InternalDraftDataController(IDraftDataService draftDataService)
        {
            _draftDataService = draftDataService;
        }

        [HttpDelete]
        public async Task<bool> DeleteDraftData([FromQuery] int objectTypeId, [FromQuery] long objectId)
        {
            return await _draftDataService.DeleteDraftData(objectTypeId, objectId);
        }
    }
}