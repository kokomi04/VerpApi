using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Manafacturing.Model.DraftData;
using VErp.Services.Manafacturing.Service.DraftData;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftDataController : VErpBaseController
    {
        private readonly IDraftDataService _draftDataService;

        public DraftDataController(IDraftDataService draftDataService)
        {
            _draftDataService = draftDataService;
        }

        [HttpPut]
        [GlobalApi]
        public async Task<DraftDataModel> UpdateDraftData([FromBody] DraftDataModel data)
        {
            return await _draftDataService.UpdateDraftData(data);
        }

        [HttpGet]
        [GlobalApi]
        public async Task<DraftDataModel> GetDraftData([FromQuery] int objectTypeId, [FromQuery] long objectId)
        {
            return await _draftDataService.GetDraftData(objectTypeId, objectId);
        }

        [HttpDelete]
        [GlobalApi]
        public async Task<bool> DeleteDraftData([FromQuery] int objectTypeId, [FromQuery] long objectId)
        {
            return await _draftDataService.DeleteDraftData(objectTypeId, objectId);
        }
    }
}
