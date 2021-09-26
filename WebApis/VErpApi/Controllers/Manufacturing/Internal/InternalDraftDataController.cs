using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
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