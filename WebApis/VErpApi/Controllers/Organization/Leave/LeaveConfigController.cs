using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Leave;
using VErp.Services.Organization.Service.Leave;

namespace VErpApi.Controllers.Organization.Leave
{
    [Route("api/organization/Leave/config")]
    public class LeaveConfigController : VErpBaseController
    {
        private readonly ILeaveConfigService _leaveConfigService;

        public LeaveConfigController(ILeaveConfigService leaveConfigService)
        {
            _leaveConfigService = leaveConfigService;
        }

        [HttpGet]
        public Task<IList<LeaveConfigListModel>> Get()
        {
            return _leaveConfigService.Get();
        }

        [HttpGet("{leaveConfigId}")]
        public Task<LeaveConfigModel> Info([FromRoute] int leaveConfigId)
        {
            return _leaveConfigService.Info(leaveConfigId);

        }

        [HttpGet("default")]
        public Task<LeaveConfigModel> Default()
        {
            return _leaveConfigService.Default();
        }

        [HttpPost]
        public Task<int> Create([FromBody] LeaveConfigModel model)
        {
            return _leaveConfigService.Create(model);
        }

        [HttpPut("{leaveConfigId}")]
        public Task<bool> Update([FromRoute] int leaveConfigId, [FromBody] LeaveConfigModel model)
        {
            return _leaveConfigService.Update(leaveConfigId, model);
        }

        [HttpDelete("{leaveConfigId}")]
        public Task<bool> Delete([FromRoute] int leaveConfigId)
        {
            return _leaveConfigService.Delete(leaveConfigId);
        }


    }
}