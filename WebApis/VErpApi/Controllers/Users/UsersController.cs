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

        /// <summary>
        /// Tìm kiếm user
        /// </summary>
        /// <param name="keyword">Từ khóa</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="size">Kích thước trang</param>
        /// <returns>
        /// </returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<UserInfoOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới user
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> Post([FromBody] UserInfoInput req)
        {
            return await _userService.CreateUser(req);
        }


        /// <summary>
        /// Lấy thông tin user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<ApiResponse<UserInfoOutput>> UserInfo([FromRoute] int userId)
        {
            return await _userService.GetInfo(userId);
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{userId}")]
        public async Task<ApiResponse> Update([FromRoute] int userId, [FromBody] UserInfoInput req)
        {
            return await _userService.UpdateUser(userId, req);
        }

        /// <summary>
        /// Xóa user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{userId}")]
        public async Task<ApiResponse> DeleteUser([FromRoute] int userId)
        {
            return await _userService.DeleteUser(userId);
        }
    }
}