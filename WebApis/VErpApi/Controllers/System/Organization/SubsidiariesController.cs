using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.Deparment;
using Services.Organization.Service.Department;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/subsidiaries")]
    public class SubsidiariesController : VErpBaseController
    {
        private readonly ISubsidiaryService _subsidiaryService;

        public SubsidiariesController(ISubsidiaryService subsidiaryService)
        {
            _subsidiaryService = subsidiaryService;
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<SubsidiaryOutput>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _subsidiaryService.GetList(keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("forLogin")]
        [AllowAnonymous]
        public async Task<IList<SubsidiaryOutput>> GetSubsidiaryForLogin()
        {
            return await _subsidiaryService.GetList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Create([FromBody] SubsidiaryModel data)
        {
            return await _subsidiaryService.Create(data).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{subsidiaryId}")]
        public async Task<SubsidiaryModel> Info([FromRoute] int subsidiaryId)
        {
            return await _subsidiaryService.GetInfo(subsidiaryId).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{subsidiaryId}")]
        public async Task<bool> Update([FromRoute] int subsidiaryId, [FromBody] SubsidiaryModel data)
        {
            return await _subsidiaryService.Update(subsidiaryId, data).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{subsidiaryId}")]
        public async Task<bool> Delete([FromRoute] int subsidiaryId)
        {
            return await _subsidiaryService.Delete(subsidiaryId).ConfigureAwait(true);
        }
    }
}