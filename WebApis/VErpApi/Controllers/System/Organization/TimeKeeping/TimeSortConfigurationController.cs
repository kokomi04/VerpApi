using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
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
        public async Task<long> AddTimeSortConfiguration([FromBody] TimeSortConfigurationModel model)
        {
            return await _timeSortConfigurationService.AddTimeSortConfiguration(model);
        }

        [HttpDelete]
        [Route("{timeSortConfigurationId}")]
        public async Task<bool> DeleteTimeSortConfiguration([FromRoute] int timeSortConfigurationId)
        {
            return await _timeSortConfigurationService.DeleteTimeSortConfiguration(timeSortConfigurationId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<TimeSortConfigurationModel>> GetListTimeSortConfiguration()
        {
            return await _timeSortConfigurationService.GetListTimeSortConfiguration();
        }

        [HttpGet]
        [Route("{timeSortConfigurationId}")]
        public async Task<TimeSortConfigurationModel> GetTimeSortConfiguration([FromRoute] int timeSortConfigurationId)
        {
            return await _timeSortConfigurationService.GetTimeSortConfiguration(timeSortConfigurationId);
        }

        [HttpPut]
        [Route("{timeSortConfigurationId}")]
        public async Task<bool> UpdateTimeSortConfiguration([FromRoute] int timeSortConfigurationId, [FromBody] TimeSortConfigurationModel model)
        {
            return await _timeSortConfigurationService.UpdateTimeSortConfiguration(timeSortConfigurationId, model);

        }
    }
}