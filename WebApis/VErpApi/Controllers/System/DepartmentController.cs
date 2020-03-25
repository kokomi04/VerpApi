﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Customer;
using VErp.Services.Master.Service.Department;
using VErp.Services.Master.Model.Department;

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
        public async Task<ApiResponse<PageData<DepartmentModel>>> Get([FromQuery] string keyword, [FromQuery] bool isActived, [FromQuery] int page, [FromQuery] int size)
        {
            return await _departmentService.GetList(keyword, isActived, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddDepartment([FromBody] DepartmentModel department)
        {
            var updatedUserId = UserId;
            return await _departmentService.AddDepartment(updatedUserId, department);
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<ApiResponse<DepartmentModel>> GetDepartmentInfo([FromRoute] int departmentId)
        {
            return await _departmentService.GetDepartmentInfo(departmentId);
        }

        [HttpPut]
        [Route("{departmentId}")]
        public async Task<ApiResponse> UpdateDepartment([FromRoute] int departmentId, [FromBody] DepartmentModel department)
        {
            var updatedUserId = UserId;
            return await _departmentService.UpdateDepartment(updatedUserId, departmentId, department);
        }

        [HttpDelete]
        [Route("{departmentId}")]
        public async Task<ApiResponse> DeleteDepartment([FromRoute] int departmentId)
        {
            var updatedUserId = UserId;
            return await _departmentService.DeleteDepartment(updatedUserId, departmentId);
        }
    }
}