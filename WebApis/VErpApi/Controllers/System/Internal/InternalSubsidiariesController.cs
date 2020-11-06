using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.Deparment;
using Services.Organization.Service.Department;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Model.Department;
using VErp.Services.Organization.Service.Customer;
using VErp.Services.Organization.Service.Department;

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
        [VErpAction(EnumAction.View)]
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