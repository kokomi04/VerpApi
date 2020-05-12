using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.System
{

    [Route("api/users")]
    public class UsersController : VErpBaseController
    {
        private readonly IUserService _userService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IFileService _fileService;
        public UsersController(IUserService userService
            , IObjectGenCodeService objectGenCodeService
            , IFileService fileService
            )
        {
            _userService = userService;
            _objectGenCodeService = objectGenCodeService;
            _fileService = fileService;
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
        public async Task<ServiceResult<PageData<UserInfoOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetList(keyword, page, size).ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách users theo ids
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetListByUserIds")]
        public async Task<IList<UserInfoOutput>> GetListByUserIds([FromBody] IList<int> userIds)
        {
            return await _userService.GetListByUserIds(userIds).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới user
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> Post([FromBody] UserInfoInput req)
        {
            int updatedUserId = UserId;
            return await _userService.CreateUser(req, updatedUserId).ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy thông tin user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<ServiceResult<UserInfoOutput>> UserInfo([FromRoute] int userId)
        {
            return await _userService.GetInfo(userId).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{userId}")]
        public async Task<ServiceResult> Update([FromRoute] int userId, [FromBody] UserInfoInput req)
        {
            int updatedUserId = UserId;
            return await _userService.UpdateUser(userId, req, updatedUserId).ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{userId}")]
        public async Task<ServiceResult> DeleteUser([FromRoute] int userId)
        {
            return await _userService.DeleteUser(userId).ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách user đc quyền truy cập vào moduleId input
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns>Danh sách người dùng</returns>
        [HttpGet]
        [Route("GetListByModuleId")]
        public async Task<ServiceResult<PageData<UserInfoOutput>>> GetListByModuleId([FromQuery] int moduleId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _userService.GetListByModuleId(UserId, moduleId, keyword, page, size).ConfigureAwait(true);
        }

        /// <summary>
        /// Upload avatar
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("avatar")]
        public async Task<ServiceResult<long>> Avatar([FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.UserAndEmployee, EnumFileType.Image, string.Empty, file).ConfigureAwait(true);
        }

        /// <summary>
        /// Sinh mã nhân viên
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateUserCode")]
        public async Task<ServiceResult<string>> GenerateUserCode()
        {
            return await _objectGenCodeService.GenerateCode(EnumObjectType.UserAndEmployee).ConfigureAwait(true);
        }
    }
}