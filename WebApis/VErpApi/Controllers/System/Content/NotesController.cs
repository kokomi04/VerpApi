using System;
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
        public async Task<bool> AddNote(AddNoteInput req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _activityService.CreateUserActivityLog(req.ObjectId, (int)req.ObjectTypeId, UserId, SubsidiaryId, (int)EnumActionType.View, EnumMessageType.Comment, req.Message);
        }

        /// <summary>
        /// Lấy danh sách ghi chú 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<UserActivityLogOuputModel>> GetNoteList([FromQuery] EnumObjectType objectTypeId, [FromQuery] long objectId, int page = 1, int size = 20)
        {
            return await _activityService.GetListUserActivityLog(objectId, objectTypeId, page, size);
        }
    }
}