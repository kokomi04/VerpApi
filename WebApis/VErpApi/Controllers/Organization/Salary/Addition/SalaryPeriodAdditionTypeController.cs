using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary.Addition
{
    [Route("api/organization/salary/addition/types")]
    public class SalaryPeriodAdditionTypeController : VErpBaseController
    {
        private readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;

        public SalaryPeriodAdditionTypeController(ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService)
        {
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
        }



        [HttpGet("")]
        public async Task<IEnumerable<SalaryPeriodAdditionTypeInfo>> GetList()
        {
            return await _salaryPeriodAdditionTypeService.List();
        }


        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryPeriodAdditionTypeModel model)
        {
            return await _salaryPeriodAdditionTypeService.Create(model);
        }


        [HttpGet("{salaryPeriodAdditionTypeId}")]
        public async Task<SalaryPeriodAdditionTypeInfo> GetInfo([FromRoute] int salaryPeriodAdditionTypeId)
        {
            return await _salaryPeriodAdditionTypeService.GetInfo(salaryPeriodAdditionTypeId);
        }

        [HttpPut("{salaryPeriodAdditionTypeId}")]
        public async Task<bool> Update([FromRoute] int salaryPeriodAdditionTypeId, [FromBody] SalaryPeriodAdditionTypeModel model)
        {
            return await _salaryPeriodAdditionTypeService.Update(salaryPeriodAdditionTypeId, model);
        }

        [HttpDelete("{salaryPeriodAdditionTypeId}")]
        public async Task<bool> Delete([FromRoute] int salaryPeriodAdditionTypeId)
        {
            return await _salaryPeriodAdditionTypeService.Delete(salaryPeriodAdditionTypeId);
        }
    }
}