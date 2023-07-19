using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Employee;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Salary
{
    [Route("api/organization/salary/employee")]
    public class SalaryEmployeeController : VErpBaseController
    {
        private readonly ISalaryEmployeeService _salaryEmployeeService;

        public SalaryEmployeeController(ISalaryEmployeeService salaryEmployeeService)
        {
            _salaryEmployeeService = salaryEmployeeService;
        }


        [HttpGet("warnings")]
        public async Task<GroupSalaryEmployeeWarningInfo> GetSalaryGroupEmployeesWarning()
        {
            return await _salaryEmployeeService.GetSalaryGroupEmployeesWarning();
        }



        [HttpPost("periods/{salaryPeriodId}/groups/{salaryGroupId}/Eval")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> EvalSalaryEmployeeByGroup([FromRoute] int salaryPeriodId, [FromRoute] int salaryGroupId, [FromBody] GroupSalaryEmployeeModel req)
        {
            return await _salaryEmployeeService.EvalSalaryEmployeeByGroup(salaryPeriodId, salaryGroupId, req);
        }

        [HttpGet("periods/{salaryPeriodId}/groups/{salaryGroupId}/GetData")]
        public async Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetSalaryEmployeeByGroup([FromRoute] int salaryPeriodId, [FromRoute] int salaryGroupId)
        {
            return await _salaryEmployeeService.GetSalaryEmployeeByGroup(salaryPeriodId, salaryGroupId);
        }

        [HttpGet("periods/{salaryPeriodId}/AllData")]
        public async Task<IList<GroupSalaryEmployeeEvalData>> GetSalaryEmployeeAll([FromRoute] int salaryPeriodId)
        {
            return await _salaryEmployeeService.GetSalaryEmployeeAll(salaryPeriodId);
        }


        [HttpPut("periods/{salaryPeriodId}/groups/{salaryGroupId}/UpdateData")]
        public async Task<bool> Update([FromRoute] int salaryPeriodId, [FromRoute] int salaryGroupId, [FromBody] GroupSalaryEmployeeModel req)
        {
            return await _salaryEmployeeService.Update(salaryPeriodId, salaryGroupId, req);
        }

        [HttpPost("group/employees")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<NonCamelCaseDictionary>> GetEmployeeInfoFromGroupSalary([FromBody] EmployeeFilterModel req)
        {

            return await _salaryEmployeeService.GetEmployeeGroupInfo(req.Filters, req.Page, req.Size);
        }
    }
}