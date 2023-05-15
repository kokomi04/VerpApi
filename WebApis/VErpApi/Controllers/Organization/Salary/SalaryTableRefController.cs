using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary
{
    [Route("api/organization/salary/refTables")]
    public class SalaryTableRefController : VErpBaseController
    {
        private readonly ISalaryRefTableService _salaryRefTableService;

        public SalaryTableRefController(ISalaryRefTableService salaryRefTableService)
        {
            _salaryRefTableService = salaryRefTableService;
        }



        [HttpGet("")]
        public async Task<IList<SalaryRefTableModel>> GetList()
        {
            return await _salaryRefTableService.GetList();
        }

        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryRefTableModel model)
        {
            return await _salaryRefTableService.Create(model);
        }

        [HttpPut("{salaryRefTableId}")]
        public async Task<bool> Update([FromRoute] int salaryRefTableId, [FromBody] SalaryRefTableModel model)
        {
            return await _salaryRefTableService.Update(salaryRefTableId, model);
        }

        [HttpDelete("{salaryRefTableId}")]
        public async Task<bool> Delete([FromRoute] int salaryRefTableId)
        {
            return await _salaryRefTableService.Delete(salaryRefTableId);
        }
    }
}