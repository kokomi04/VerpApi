using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.System
{

    [Route("api/CustomGenCodeConfigs")]
    public class CustomGenCodeController : VErpBaseController
    {
        private readonly ICustomGenCodeService _customGenCodeService;
        public CustomGenCodeController(ICustomGenCodeService customGenCodeService
            )
        {
            _customGenCodeService = customGenCodeService;
        }

        /// <summary>
        /// Lấy danh sách cấu hình gen code
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<CustomGenCodeOutputModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? objectTypeId)
        {
            return await _customGenCodeService.GetList(keyword, page, size, objectTypeId);
        }

        /// <summary>
        /// Lấy thông tin cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="customGenCodeId">Id cấu hình</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{customGenCodeId}")]
        public async Task<ApiResponse<CustomGenCodeOutputModel>> GetInfo([FromRoute] int customGenCodeId)
        {
            return await _customGenCodeService.GetInfo(customGenCodeId);
        }


        /// <summary>
        /// Thêm mới cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> Post([FromBody] CustomGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _customGenCodeService.Create(currentId, req);
        }


        /// <summary>
        /// Cập nhật cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="customGenCodeId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{customGenCodeId}")]
        public async Task<ApiResponse> Update([FromRoute] int customGenCodeId, [FromBody] CustomGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _customGenCodeService.Update(customGenCodeId, currentId,req);
        }


        /// <summary>
        /// Xóa cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="customGenCodeId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{customGenCodeId}")]
        public async Task<ApiResponse> Delete([FromRoute] int customGenCodeId)
        {
            var currentId = UserId;
            return await _customGenCodeService.Delete(currentId, customGenCodeId);
        }

        /// <summary>
        /// Tạo mã code
        /// </summary>
        /// <param name="objectType">Loại đối tượng cần tạo mã code</param>
        /// <returns>string Code</returns>
        [HttpGet]
        [Route("GenerateCode")]
        public async Task<ApiResponse<string>> GenerateCode([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _customGenCodeService.GenerateCode(objectTypeId, objectId);
        }

        /// <summary>
        /// Lấy danh sách các loại đối tượng 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllObjectType")]
        public async Task<ApiResponse<PageData<ObjectType>>> GetAllObjectType()
        {
            return await _customGenCodeService.GetAllObjectType();
        }
    }
}