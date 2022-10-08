using Microsoft.AspNetCore.Mvc;
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

    [Route("api/UserActivityLog")]
    public class UserActivityLogController : VErpBaseController
    {
        private readonly IActivityService _activityService;
        public UserActivityLogController(IActivityService activityService)
        {
            _activityService = activityService;
        }



        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<PageData<UserActivityLogOuputModel>> GetNoteList([FromQuery] string keyword, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int? userId, [FromQuery] int? billTypeId, [FromQuery] long? objectId, [FromQuery] EnumObjectType? objectTypeId, [FromQuery] int? actionTypeId, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            return await _activityService.GetListUserActivityLog(null, keyword, fromDate, toDate, userId, billTypeId, objectId, objectTypeId, actionTypeId, sortBy, asc, page, size);
        }


    }
}