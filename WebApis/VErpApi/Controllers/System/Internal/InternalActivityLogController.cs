using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalActivityLogController : CrossServiceBaseController
    {
        private readonly IActivityService _activityService;
        public InternalActivityLogController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [Route("Log")]
        [HttpPost]
        public Task<ApiResponse> Log([FromBody] ActivityInput req)
        {
            _activityService.CreateActivityAsync(req);
            return Task.FromResult((ApiResponse)GeneralCode.Success);
        }
    }
}