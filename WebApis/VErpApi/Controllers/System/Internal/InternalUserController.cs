using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.System.Internal
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
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<UserInfoOutput>> Get([FromBody] Clause filters, [FromQuery] IList<int> userIds, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetList(keyword, userIds, page, size, filters).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetByIds")]
        public async Task<IList<UserInfoOutput>> GetByIds([FromBody] IList<int> userIds)
        {
            return await _userService.GetListByUserIds(userIds).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<UserInfoOutput> UserInfo([FromRoute] int userId)
        {
            return await _userService.GetInfo(userId).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách users theo roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        [HttpPost]
        [GlobalApi]
        [Route("GetListByRoles")]
        public async Task<IList<UserInfoOutput>> GetListByRoles([FromBody] IList<int> roles)
        {
            return await _userService.GetListByRoleIds(roles).ConfigureAwait(true);
        }
    }
}