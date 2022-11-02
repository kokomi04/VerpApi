using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalActivityLogController : CrossServiceBaseController
    {
        private readonly IUserLogActionService _userLogActionService;
        public InternalActivityLogController(IUserLogActionService userLogActionService)
        {
            _userLogActionService = userLogActionService;
        }

        [Route("Log")]
        [HttpPost]
        public bool Log([FromBody] ActivityInput req)
        {
            _userLogActionService.CreateActivityAsync(req);
            return true;
        }

    }
}