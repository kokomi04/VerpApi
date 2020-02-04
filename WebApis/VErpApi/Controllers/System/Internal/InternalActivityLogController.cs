using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Services.Master.Service.Activity;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalActivityLogController : CrossServiceBaseController
    {
        private readonly IActivityService _activityService;
        public InternalActivityLogController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [Route("Log")]
        [HttpPost]
        public async Task<ApiResponse> ChangePassword([FromBody] UserChangepasswordInput req)
        {
            return await _activityService.CreateActivityAsync(UserId, req);
        }
    }
}