using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Hr.Salary;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.Salary;

namespace VErpApi.Controllers.Organization.Internal.Salary
{


    [Route("api/internal/[controller]")]
    public class InternalSalaryPeriodAdditionTypeController : CrossServiceBaseController
    {
        public readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;

        public InternalSalaryPeriodAdditionTypeController(ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService)
        {
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
        }

        [HttpGet("")]
        public async Task<IEnumerable<ISalaryPeriodAddtionTypeBase>> GetList()
        {
            return await _salaryPeriodAdditionTypeService.List();
        }
    }
}
