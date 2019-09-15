using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users.Interface;

namespace VErpApi.Controllers.Users
{

    [Route("api/users")]
    public class UsersController : VErpBaseController
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService
            )
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<UserInfoOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetList(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> Post([FromBody] UserInfoInput req)
        {
            return await _userService.CreateUser(req);
        }


        [HttpGet]
        [Route("{userId}")]
        public async Task<ApiResponse<UserInfoOutput>> UserInfo([FromRoute] int userId)
        {
            return await _userService.GetInfo(userId);
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task<ApiResponse> Update([FromRoute] int userId, [FromBody] UserInfoInput req)
        {
            return await _userService.UpdateUser(userId, req);
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task<ApiResponse> Update([FromRoute] int userId)
        {
            return await _userService.DeleteUser(userId);
        }
    }
}