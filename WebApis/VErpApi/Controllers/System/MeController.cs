using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users.Interface;

namespace VErpApi.Controllers.System
{
    [Route("api/users/me")]
    public class MeController : VErpBaseController
    {
        private readonly IUserService _userService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrant;
        public MeController(IUserService userService,
            IIdentityServerInteractionService interaction,
            IPersistedGrantService persistedGrant
            )
        {
            _userService = userService;
            _interaction = interaction;
            _persistedGrant = persistedGrant;
        }

        [Route("info")]
        [HttpGet]
        public async Task<ApiResponse<UserInfoOutput>> GetInfo()
        {
            return await _userService.GetInfo(UserId);
        }

        [Route("censor")]
        [HttpPost]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse<User>> TestAction()
        {
            throw new NotImplementedException("Test http post as censor!");
        }

        [Route("logout")]
        [HttpPost]
        public async Task<ApiResponse> Logout()
        {
            var asa = await _persistedGrant.GetAllGrantsAsync(Sub);

            await _persistedGrant.RemoveAllGrantsAsync(Sub, ClientId);

            return GeneralCode.Success;
        }

        [Route("permissions")]
        [HttpGet]
        public async Task<ApiResponse<IList<RolePermissionModel>>> GetPermission()
        {
            return (await _userService.GetUserPermission(UserId)).ToList();
        }
    }
}