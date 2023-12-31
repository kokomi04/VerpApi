﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Department;
using VErp.Services.Organization.Service.Department;

namespace VErpApi.Controllers.System.Internal
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
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<DepartmentExtendModel>> Get(
            [FromQuery] string keyword, 
            [FromQuery] IList<int> departmentIds, 
            [FromQuery] bool? isProduction, 
            [FromQuery] bool? isActived, 
            [FromQuery] int page, 
            [FromQuery] int size,
            [FromQuery] string orderByFieldName = nameof(DepartmentExtendModel.DepartmentCode),
            [FromQuery] bool asc = true, 
            [FromBody] Clause filters = null)
        {
            return await _departmentService.GetList(keyword, departmentIds, isProduction, isActived, page, size, orderByFieldName, asc, filters);
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<DepartmentModel> GetDepartmentInfo([FromRoute] int departmentId)
        {
            return await _departmentService.GetDepartmentInfo(departmentId);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetByIds")]
        public async Task<IList<DepartmentModel>> GetByIds([FromBody] IList<int> departmentIds)
        {
            return await _departmentService.GetListByIds(departmentIds);
        }
    }
}