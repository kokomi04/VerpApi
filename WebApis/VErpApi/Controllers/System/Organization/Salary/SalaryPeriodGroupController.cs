using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.System.Organization.Salary
{
    [Route("api/organization/salary/periodGroups")]
    public class SalaryPeriodGroupController : VErpBaseController
    {
        private readonly ISalaryPeriodGroupService _salaryPeriodGroupService;

        public SalaryPeriodGroupController(ISalaryPeriodGroupService salaryPeriodGroupService)
        {
            _salaryPeriodGroupService = salaryPeriodGroupService;
        }


        [HttpGet("")]
        public async Task<IList<SalaryPeriodGroupInfo>> GetList([FromQuery] int salaryPeriodId)
        {
            return await _salaryPeriodGroupService.GetList(salaryPeriodId);
        }

        [HttpGet("{salaryPeriodGroupId}")]
        public async Task<SalaryPeriodGroupInfo> GetInfo([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.GetInfo(salaryPeriodGroupId);
        }

        [HttpGet("GetInfoByGroup")]
        public async Task<SalaryPeriodGroupInfo> GetInfoByGroup([FromQuery] int salaryPeriodId, [FromQuery] int salaryGroupId)
        {
            return await _salaryPeriodGroupService.GetInfo(salaryPeriodId, salaryGroupId);
        }


        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryPeriodGroupModel model)
        {
            return await _salaryPeriodGroupService.Create(model);
        }

        [HttpPut("{salaryPeriodGroupId}")]
        public async Task<bool> Update([FromRoute] long salaryPeriodGroupId, [FromBody] SalaryPeriodGroupModel model)
        {
            return await _salaryPeriodGroupService.Update(salaryPeriodGroupId, model);
        }

        [HttpDelete("{salaryPeriodGroupId}")]
        public async Task<bool> Delete([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.Delete(salaryPeriodGroupId);
        }

        [HttpPut("{salaryPeriodGroupId}/CheckAccept")]
        public async Task<bool> CheckAccept([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.Check(salaryPeriodGroupId, true);
        }

        [HttpPut("{salaryPeriodGroupId}/CheckReject")]
        public async Task<bool> CheckReject([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.Check(salaryPeriodGroupId, false);
        }


        [HttpPut("{salaryPeriodGroupId}/CensorApprove")]
        public async Task<bool> CensorApprove([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.Censor(salaryPeriodGroupId, true);
        }


        [HttpPut("{salaryPeriodGroupId}/CensorReject")]
        public async Task<bool> CensorReject([FromRoute] long salaryPeriodGroupId)
        {
            return await _salaryPeriodGroupService.Censor(salaryPeriodGroupId, false);
        }

    }
}