using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary
{
    [Route("api/organization/salary/fields")]
    public class SalaryFieldController : VErpBaseController
    {
        private readonly ISalaryFieldService _salaryFieldService;

        public SalaryFieldController(ISalaryFieldService salaryFieldService)
        {
            _salaryFieldService = salaryFieldService;
        }



        [HttpGet("")]
        public async Task<IList<SalaryFieldModel>> GetList()
        {
            return await _salaryFieldService.GetList();
        }

        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryFieldModel model)
        {
            return await _salaryFieldService.Create(model);
        }

        [HttpPut("{salaryFieldId}")]
        public async Task<bool> Update([FromRoute] int salaryFieldId, [FromBody] SalaryFieldModel model)
        {
            return await _salaryFieldService.Update(salaryFieldId, model);
        }

        [HttpDelete("{salaryFieldId}")]
        public async Task<bool> Delete([FromRoute] int salaryFieldId)
        {
            return await _salaryFieldService.Delete(salaryFieldId);
        }
    }
}