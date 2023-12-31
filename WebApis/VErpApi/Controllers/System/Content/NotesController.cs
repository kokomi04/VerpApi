﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.Master.Service.Activity;

namespace VErpApi.Controllers.System
{

    [Route("api/notes")]
    public class NotesController : VErpBaseController
    {
        private readonly IUserLogActionService _userLogActionService;
        public NotesController(IUserLogActionService userLogActionService)
        {
            _userLogActionService = userLogActionService;
        }

        /// <summary>
        /// Thêm ghi chú
        /// </summary>    
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [GlobalApi]
        public async Task<bool> AddNote(AddNoteInput req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _userLogActionService.AddNote(req.BillTypeId, req.ObjectId, (int)req.ObjectTypeId, req.Message);
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<PageData<UserActivityLogOuputModel>> GetNoteList([FromQuery] int? billTypeId, [FromQuery] EnumObjectType objectTypeId, [FromQuery] long objectId, int page = 1, int size = 20)
        {
            return await _userLogActionService.GetUserLogByObject(billTypeId, objectId, objectTypeId, page, size);
        }

        [HttpPost]
        [Route("byArrayId")]
        [GlobalApi]
        public async Task<IList<UserActivityLogOuputModel>> GetNoteList([FromBody] long[] userActivityLogIds)
        {
            var pagedData = await _userLogActionService.GetListUserActivityLog(userActivityLogIds, null, null, null, null, null, null, null, null, null, false);
            return pagedData.List;
        }

        [HttpPost]
        [Route("loginLog")]
        public async Task<PageData<UserLoginLogModel>> GetUserLoginLogs([FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string keyword,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromBody] Clause filter)
        {
            return await _userLogActionService.GetUserLoginLogs(page, size, keyword, orderByFieldName, asc, fromDate, toDate, filter);
        }
    }
}