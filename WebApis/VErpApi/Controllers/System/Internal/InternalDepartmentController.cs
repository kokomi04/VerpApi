using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Model.Department;
using VErp.Services.Organization.Service.Customer;
using VErp.Services.Organization.Service.Department;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalDepartmentController : CrossServiceBaseController
    {
        private readonly IDepartmentService _departmentService;
        public InternalDepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("")]
        public async Task<PageData<DepartmentModel>> Get([FromQuery] string keyword, [FromQuery] bool? isActived, [FromQuery] int page, [FromQuery] int size, [FromBody] Clause filters = null)
        {
            return await _departmentService.GetList(keyword, isActived, page, size, filters);
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<DepartmentModel> GetDepartmentInfo([FromRoute] int departmentId)
        {
            return await _departmentService.GetDepartmentInfo(departmentId);
        }
    }
}