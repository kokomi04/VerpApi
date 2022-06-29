using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/overtimeLevel")]
    public class OvertimeLevelController : VErpBaseController
    {
        private readonly IOvertimeLevelService _countedSymbolService;

        public OvertimeLevelController(IOvertimeLevelService countedSymbolService)
        {
            _countedSymbolService = countedSymbolService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddOvertimeLevel([FromBody] OvertimeLevelModel model)
        {
            return await _countedSymbolService.AddOvertimeLevel(model);
        }

        [HttpDelete]
        [Route("{overtimeLevelId}")]
        public async Task<bool> DeleteOvertimeLevel([FromRoute] int overtimeLevelId)
        {
            return await _countedSymbolService.DeleteOvertimeLevel(overtimeLevelId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<OvertimeLevelModel>> GetListOvertimeLevel()
        {
            return await _countedSymbolService.GetListOvertimeLevel();
        }

        [HttpGet]
        [Route("{overtimeLevelId}")]
        public async Task<OvertimeLevelModel> GetOvertimeLevel([FromRoute] int overtimeLevelId)
        {
            return await _countedSymbolService.GetOvertimeLevel(overtimeLevelId);
        }

        [HttpPut]
        [Route("{overtimeLevelId}")]
        public async Task<bool> UpdateOvertimeLevel([FromRoute] int overtimeLevelId, [FromBody] OvertimeLevelModel model)
        {
            return await _countedSymbolService.UpdateOvertimeLevel(overtimeLevelId, model);

        }
    }
}