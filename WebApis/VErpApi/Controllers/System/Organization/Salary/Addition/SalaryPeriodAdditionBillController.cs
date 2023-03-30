using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.System.Organization.Salary.Addition
{
    [Route("api/organization/salary/addition/data")]
    public class SalaryPeriodAdditionBillController : VErpBaseController
    {
        private readonly ISalaryPeriodAdditionBillService _salaryPeriodAdditionBillService;

        public SalaryPeriodAdditionBillController(ISalaryPeriodAdditionBillService salaryPeriodAdditionBillService)
        {
            _salaryPeriodAdditionBillService = salaryPeriodAdditionBillService;
        }


        [HttpGet("{salaryPeriodAdditionTypeId}/bills")]
        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList([FromRoute] int salaryPeriodAdditionTypeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int page, [FromQuery] int size)
        {
            return await _salaryPeriodAdditionBillService.GetList(salaryPeriodAdditionTypeId, year, month, page, size);
        }

        [HttpPost("{salaryPeriodAdditionTypeId}/bills")]
        public async Task<long> Create([FromRoute] int salaryPeriodAdditionTypeId, [FromBody] SalaryPeriodAdditionBillModel model)
        {
            return await _salaryPeriodAdditionBillService.Create(salaryPeriodAdditionTypeId, model);
        }

        [HttpPut("{salaryPeriodAdditionTypeId}/bills/{salaryPeriodAdditionBillId}")]
        public async Task<bool> Update([FromRoute] int salaryPeriodAdditionTypeId, [FromRoute] int salaryPeriodAdditionBillId, [FromBody] SalaryPeriodAdditionBillModel model)
        {
            return await _salaryPeriodAdditionBillService.Update(salaryPeriodAdditionTypeId, salaryPeriodAdditionBillId, model);
        }

        [HttpDelete("{salaryPeriodAdditionTypeId}/bills/{salaryPeriodAdditionBillId}")]
        public async Task<bool> Delete([FromRoute] int salaryPeriodAdditionTypeId, [FromRoute] int salaryPeriodAdditionBillId)
        {
            return await _salaryPeriodAdditionBillService.Delete(salaryPeriodAdditionTypeId, salaryPeriodAdditionBillId);
        }
    }
}