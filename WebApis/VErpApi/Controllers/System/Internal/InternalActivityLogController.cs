using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
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
        public bool Log([FromBody] ActivityInput req)
        {
            _activityService.CreateActivityAsync(req);
            return true;
        }

    }
}