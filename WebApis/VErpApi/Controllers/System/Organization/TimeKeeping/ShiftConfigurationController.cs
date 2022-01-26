using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.SystemParameter;
using Services.Organization.Model.TimeKeeping;
using Services.Organization.Service.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/shiftConfiguration")]
    public class ShiftConfigurationController: VErpBaseController
    {
        private readonly IShiftConfigurationService _shiftConfigurationService;

        public ShiftConfigurationController(IShiftConfigurationService shiftConfigurationService)
        {
            _shiftConfigurationService = shiftConfigurationService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddShiftConfiguration([FromBody]ShiftConfigurationModel model)
        {
            return await _shiftConfigurationService.AddShiftConfiguration(model);
        }
        
        [HttpDelete]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> DeleteShiftConfiguration([FromRoute]int shiftConfigurationId)
        {
            return await _shiftConfigurationService.DeleteShiftConfiguration(shiftConfigurationId);
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<ShiftConfigurationModel>> GetListShiftConfiguration()
        {
            return await _shiftConfigurationService.GetListShiftConfiguration();
        }
        
        [HttpGet]
        [Route("{shiftConfigurationId}")]
        public async Task<ShiftConfigurationModel> GetShiftConfiguration([FromRoute]int shiftConfigurationId)
        {
            return await _shiftConfigurationService.GetShiftConfiguration(shiftConfigurationId);
        }
        
        [HttpPut]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> UpdateShiftConfiguration([FromRoute] int shiftConfigurationId, [FromBody]ShiftConfigurationModel model)
        {
            return await _shiftConfigurationService.UpdateShiftConfiguration(shiftConfigurationId, model);

        }
    }
}