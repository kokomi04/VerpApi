﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.System
{

    [Route("api/notes")]
    public class NotesController : VErpBaseController
    {
        private readonly IActivityService _activityService;
        public NotesController(IActivityService activityService)
        {
            _activityService = activityService;
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
            return await _activityService.AddNote(req.BillTypeId, req.ObjectId, (int)req.ObjectTypeId, req.Message);
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<PageData<UserActivityLogOuputModel>> GetNoteList([FromQuery] int? billTypeId, [FromQuery] EnumObjectType objectTypeId, [FromQuery] long objectId, int page = 1, int size = 20)
        {
            return await _activityService.GetListUserActivityLog(billTypeId, objectId, objectTypeId, page, size);
        }
    }
}