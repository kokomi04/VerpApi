using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.Deparment;
using Services.Organization.Service.Department;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalSubsidiariesController : CrossServiceBaseController
    {
        private readonly ISubsidiaryService _subsidiaryService;
        public InternalSubsidiariesController(ISubsidiaryService subsidiaryService)
        {
            _subsidiaryService = subsidiaryService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<SubsidiaryOutput>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _subsidiaryService.GetList(keyword, page, size, filters).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{subsidiaryId}")]
        public async Task<SubsidiaryModel> Info([FromRoute] int subsidiaryId)
        {
            return await _subsidiaryService.GetInfo(subsidiaryId).ConfigureAwait(true);
        }
    }
}