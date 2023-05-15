using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary.Addition
{
    [Route("api/organization/salary/addition/fields")]
    public class SalaryPeriodAdditionFieldController : VErpBaseController
    {
        private readonly ISalaryPeriodAdditionFieldService _salaryPeriodAdditionFieldService;

        public SalaryPeriodAdditionFieldController(ISalaryPeriodAdditionFieldService salaryPeriodAdditionFieldService)
        {
            _salaryPeriodAdditionFieldService = salaryPeriodAdditionFieldService;
        }



        [HttpGet("")]
        public async Task<IList<SalaryPeriodAdditionFieldInfo>> GetList()
        {
            return await _salaryPeriodAdditionFieldService.List();
        }

        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryPeriodAdditionFieldModel model)
        {
            return await _salaryPeriodAdditionFieldService.Create(model);
        }

        [HttpPut("{salaryPeriodAdditionFieldId}")]
        public async Task<bool> Update([FromRoute] int salaryPeriodAdditionFieldId, [FromBody] SalaryPeriodAdditionFieldModel model)
        {
            return await _salaryPeriodAdditionFieldService.Update(salaryPeriodAdditionFieldId, model);
        }

        [HttpDelete("{salaryPeriodAdditionFieldId}")]
        public async Task<bool> Delete([FromRoute] int salaryPeriodAdditionFieldId)
        {
            return await _salaryPeriodAdditionFieldService.Delete(salaryPeriodAdditionFieldId);
        }
    }
}