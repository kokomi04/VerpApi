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
    [Route("api/organization/timekeeping/timeSort")]
    public class TimeSortConfigurationController : VErpBaseController
    {
        private readonly ITimeSortConfigurationService _timeSortConfigurationService;

        public TimeSortConfigurationController(ITimeSortConfigurationService timeSortConfigurationService)
        {
            _timeSortConfigurationService = timeSortConfigurationService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddTimeSortConfiguration([FromBody]TimeSortConfigurationModel model)
        {
            return await _timeSortConfigurationService.AddTimeSortConfiguration(model);
        }
        
        [HttpDelete]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> DeleteTimeSortConfiguration([FromRoute]int shiftConfigurationId)
        {
            return await _timeSortConfigurationService.DeleteTimeSortConfiguration(shiftConfigurationId);
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<TimeSortConfigurationModel>> GetListTimeSortConfiguration()
        {
            return await _timeSortConfigurationService.GetListTimeSortConfiguration();
        }
        
        [HttpGet]
        [Route("{shiftConfigurationId}")]
        public async Task<TimeSortConfigurationModel> GetTimeSortConfiguration([FromRoute]int shiftConfigurationId)
        {
            return await _timeSortConfigurationService.GetTimeSortConfiguration(shiftConfigurationId);
        }
        
        [HttpPut]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> UpdateTimeSortConfiguration([FromRoute] int shiftConfigurationId, [FromBody]TimeSortConfigurationModel model)
        {
            return await _timeSortConfigurationService.UpdateTimeSortConfiguration(shiftConfigurationId, model);

        }
    }
}