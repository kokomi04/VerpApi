using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalUserController : CrossServiceBaseController
    {
        private readonly IUserService _userService;
        public InternalUserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("")]
        public async Task<PageData<UserInfoOutput>> Get([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetList(keyword, page, size, filters).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<UserInfoOutput> UserInfo([FromRoute] int userId)
        {
            return await _userService.GetInfo(userId).ConfigureAwait(true);
        }
    }
}