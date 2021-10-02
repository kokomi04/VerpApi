﻿using System.Threading.Tasks;
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
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.DepartmentCalendar;
using VErp.Services.Organization.Model.DepartmentCalendar;

namespace VErpApi.Controllers.System
{
    [Route("api/departmentCalendar")]
    public class DepartmentCalendarController : VErpBaseController
    {
        private readonly IDepartmentCalendarService _departmentCalendarService;

        public DepartmentCalendarController(IDepartmentCalendarService departmentCalendarService)
        {
            _departmentCalendarService = departmentCalendarService;
        }

        [HttpGet]
        [Route("{departmentId}")]
        public async Task<IList<DepartmentCalendarModel>> GetDepartmentCalendars([FromRoute] int departmentId)
        {
            return await _departmentCalendarService.GetDepartmentCalendars(departmentId);
        }

        [HttpPost]
        [Route("multiple")]
        public async Task<IList<DepartmentCalendarListModel>> GetListDepartmentCalendar([FromQuery] long startDate, [FromQuery] long endDate, [FromBody] int[] departmentIds)
        {
            return await _departmentCalendarService.GetListDepartmentCalendar(departmentIds, startDate, endDate);
        }

        [HttpGet]
        [Route("{departmentId}/over-hour")]
        public async Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo([FromRoute] int departmentId)
        {
            return await _departmentCalendarService.GetDepartmentOverHourInfo(departmentId);
        }

        [HttpPost]
        [Route("over-hour")]
        public async Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo([FromBody] int[] departmentIds)
        {
            return await _departmentCalendarService.GetDepartmentOverHourInfo(departmentIds);
        }

        [HttpPost]
        [Route("{departmentId}/over-hour")]
        public async Task<DepartmentOverHourInfoModel> CreateDepartmentOverHourInfo([FromRoute] int departmentId, [FromBody] DepartmentOverHourInfoModel data)
        {
            return await _departmentCalendarService.CreateDepartmentOverHourInfo(departmentId, data);
        }

        [HttpPut]
        [Route("{departmentId}/over-hour/{departmentOverHourInfoId}")]
        public async Task<DepartmentOverHourInfoModel> UpdateDepartmentOverHourInfo([FromRoute] int departmentId, [FromRoute] long departmentOverHourInfoId, [FromBody] DepartmentOverHourInfoModel data)
        {
            return await _departmentCalendarService.UpdateDepartmentOverHourInfo(departmentId, departmentOverHourInfoId, data);
        }

        [HttpDelete]
        [Route("{departmentId}/over-hour/{departmentOverHourInfoId}")]
        public async Task<bool> DeleteDepartmentOverHourInfo([FromRoute] int departmentId, [FromRoute] long departmentOverHourInfoId)
        {
            return await _departmentCalendarService.DeleteDepartmentOverHourInfo(departmentId, departmentOverHourInfoId);
        }

        [HttpPut]
        [Route("over-hour/multiple")]
        public async Task<IList<DepartmentOverHourInfoModel>> UpdateDepartmentOverHourInfoMultiple([FromBody] IList<DepartmentOverHourInfoModel> data)
        {
            return await _departmentCalendarService.UpdateDepartmentOverHourInfoMultiple(data);
        }
    }
}