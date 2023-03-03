using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.System.Organization.Salary
{
    [Route("api/organization/salary/groups")]
    public class SalaryGroupController : VErpBaseController
    {
        private readonly ISalaryGroupService _salaryGroupService;

        public SalaryGroupController(ISalaryGroupService salaryTableService)
        {
            _salaryGroupService = salaryTableService;
        }



        [HttpGet("")]
        public async Task<IList<SalaryGroupModel>> GetList()
        {
            return await _salaryGroupService.GetList();
        }

        [HttpPost("")]
        public async Task<int> Create([FromBody] SalaryGroupModel model)
        {
            return await _salaryGroupService.Create(model);
        }

        [HttpPut("{salaryGroupId}")]
        public async Task<bool> Update([FromRoute] int salaryGroupId, [FromBody] SalaryGroupModel model)
        {
            return await _salaryGroupService.Update(salaryGroupId, model);
        }

        [HttpDelete("{salaryGroupId}")]
        public async Task<bool> Delete([FromRoute] int salaryGroupId)
        {
            return await _salaryGroupService.Delete(salaryGroupId);
        }
    }
}