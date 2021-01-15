using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Organization.Model.Department;
using System.Collections.Generic;

namespace VErpApi.Controllers.System
{
    [Route("api/departments")]
    public class DepartmentController : VErpBaseController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<DepartmentModel>> Get([FromQuery] string keyword, [FromQuery] bool? isActived, [FromQuery] int page, [FromQuery] int size)
        {
            return await _departmentService.GetList(keyword, isActived, page, size);
        }

        [HttpGet]
        [Route("GetByIds")]
        public async Task<IList<DepartmentModel>> GetByIds([FromQuery] IList<int> departmentIds)
        {
            return await _departmentService.GetListByIds(departmentIds);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddDepartment([FromBody] DepartmentModel department)
        {
            var updatedUserId = UserId;
            return await _departmentService.AddDepartment(updatedUserId, department);
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<DepartmentModel> GetDepartmentInfo([FromRoute] int departmentId)
        {
            return await _departmentService.GetDepartmentInfo(departmentId);
        }

        [HttpPut]
        [Route("{departmentId}")]
        public async Task<bool> UpdateDepartment([FromRoute] int departmentId, [FromBody] DepartmentModel department)
        {
            var updatedUserId = UserId;
            return await _departmentService.UpdateDepartment(updatedUserId, departmentId, department);
        }

        [HttpDelete]
        [Route("{departmentId}")]
        public async Task<bool> DeleteDepartment([FromRoute] int departmentId)
        {
            var updatedUserId = UserId;
            return await _departmentService.DeleteDepartment(updatedUserId, departmentId);
        }
    }
}