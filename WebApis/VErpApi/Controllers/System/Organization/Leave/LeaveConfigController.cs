using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Organization.Model.Department;
using System.Collections.Generic;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.Calendar;
using VErp.Services.Organization.Model.Calendar;
using VErp.Services.Organization.Service.Leave;
using VErp.Services.Organization.Model.Leave;

namespace VErpApi.Controllers.System.Organization.Leave
{
    [Route("api/Leave/config")]
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