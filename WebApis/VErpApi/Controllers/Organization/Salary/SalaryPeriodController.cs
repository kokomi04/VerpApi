using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary
{
    [Route("api/organization/salary/periods")]
    public class SalaryPeriodController : VErpBaseController
    {
        private readonly ISalaryPeriodService _salaryPeriodService;

        public SalaryPeriodController(ISalaryPeriodService salaryPeriodService)
        {
            _salaryPeriodService = salaryPeriodService;
        }


        [HttpGet("")]
        public async Task<PageData<SalaryPeriodInfo>> GetList([FromQuery] int page, [FromQuery] int size)
        {
            return await _salaryPeriodService.GetList(page, size);
        }

        [HttpGet("{salaryPeriodId}")]
        public async Task<SalaryPeriodInfo> GetInfo([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.GetInfo(salaryPeriodId);
        }

        [HttpGet("GetInfoByMonth")]
        public async Task<SalaryPeriodInfo> GetInfoByMonth([FromQuery] int year, [FromQuery] int month)
        {
            return await _salaryPeriodService.GetInfo(year, month);
        }


        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryPeriodModel model)
        {
            return await _salaryPeriodService.Create(model);
        }

        [HttpPut("{salaryPeriodId}")]
        public async Task<bool> Update([FromRoute] int salaryPeriodId, [FromBody] SalaryPeriodModel model)
        {
            return await _salaryPeriodService.Update(salaryPeriodId, model);
        }

        [HttpDelete("{salaryPeriodId}")]
        public async Task<bool> Delete([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.Delete(salaryPeriodId);
        }

        [HttpPut("{salaryPeriodId}/CheckAccept")]
        public async Task<bool> CheckAccept([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.Check(salaryPeriodId, true);
        }

        [HttpPut("{salaryPeriodId}/CheckReject")]
        public async Task<bool> CheckReject([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.Check(salaryPeriodId, false);
        }


        [HttpPut("{salaryPeriodId}/CensorApprove")]
        public async Task<bool> CensorApprove([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.Censor(salaryPeriodId, true);
        }


        [HttpPut("{salaryPeriodId}/CensorReject")]
        public async Task<bool> CensorReject([FromRoute] int salaryPeriodId)
        {
            return await _salaryPeriodService.Censor(salaryPeriodId, false);
        }

    }
}