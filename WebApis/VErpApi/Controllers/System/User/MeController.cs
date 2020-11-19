using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Organization.Model.Employee;
using Services.Organization.Service.Employee;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Filters;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.System
{
    [Route("api/users/me")]
    public class MeController : VErpBaseController
    {
        private readonly IUserService _userService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrant;
        private readonly IUserDataService _userDataService;

        public MeController(IUserService userService,
            IIdentityServerInteractionService interaction,
            IPersistedGrantService persistedGrant,
            IUserDataService userDataService
            )
        {
            _userService = userService;
            _interaction = interaction;
            _persistedGrant = persistedGrant;
            _userDataService = userDataService;
        }

        [Route("info")]
        [HttpGet]
        public async Task<UserInfoOutput> GetInfo()
        {
            return await _userService.GetInfo(UserId);
        }

        [Route("censor")]
        [HttpPost]
        [VErpAction(EnumAction.Censor)]
        public async Task<User> TestAction()
        {
            await Task.CompletedTask;
            throw new NotImplementedException("Test http post as censor!");
        }

        [Route("logout")]
        [HttpPost]
        public async Task<bool> Logout()
        {
            await _persistedGrant.GetAllGrantsAsync(Sub);

            await _persistedGrant.RemoveAllGrantsAsync(Sub, ClientId);

            return true;
        }

        /// <summary>
        /// Lấy danh sách quyền của user đang login
        /// </summary>
        /// <returns></returns>
        [Route("permissions")]
        [HttpGet]
        public async Task<IList<RolePermissionModel>> GetPermission()
        {
            return (await _userService.GetMePermission()).ToList();
        }


        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("changePassword")]
        [HttpPut]
        public async Task<bool> ChangePassword([FromBody] UserChangepasswordInput req)
        {
            return await _userService.ChangeUserPassword(UserId, req);
        }

        [Route("UserData/{key}")]
        [HttpGet]
        [GlobalApi]
        public async Task<UserDataModel> GetUserData([FromRoute] string key)
        {
            return await _userDataService.GetUserData(key);
        }

        [Route("UserData/{key}")]
        [HttpPut]
        [GlobalApi]
        public async Task<bool> UpdateUserData([FromRoute] string key, [FromBody] UserDataModel data)
        {
            return await _userDataService.UpdateUserData(key, data?.DataContent);
        }
    }
}