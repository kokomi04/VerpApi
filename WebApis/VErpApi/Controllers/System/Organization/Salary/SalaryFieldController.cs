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