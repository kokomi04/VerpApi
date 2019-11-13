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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;

namespace VErpApi.Controllers.System
{

    [Route("api/GenCodeConfigs")]
    public class ObjectGenCodeController : VErpBaseController
    {
        private readonly IObjectGenCodeService _objectGenCodeService;
        public ObjectGenCodeController(IObjectGenCodeService objectGenCodeService
            )
        {
            _objectGenCodeService = objectGenCodeService;
        }

        /// <summary>
        /// Lấy danh sách cấu hình gen code
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<ObjectGenCodeOutputModel>>> Get([FromQuery] EnumObjectType objectType, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _objectGenCodeService.GetList(objectType,keyword, page, size);
        }

        /// <summary>
        /// Lấy thông tin cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId">Id cấu hình</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{objectGenCodeId}")]
        public async Task<ApiResponse<ObjectGenCodeOutputModel>> GetInfo([FromRoute] int objectGenCodeId)
        {
            return await _objectGenCodeService.GetInfo(objectGenCodeId);
        }


        /// <summary>
        /// Thêm mới cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectType">Loại đối tượng</param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> Post([FromQuery] EnumObjectType objectType, [FromBody] ObjectGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _objectGenCodeService.Create(objectType, currentId, req);
        }


        /// <summary>
        /// Cập nhật cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{objectGenCodeId}")]
        public async Task<ApiResponse> Update([FromRoute] int objectGenCodeId, [FromBody] ObjectGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _objectGenCodeService.Update(objectGenCodeId, currentId,req);
        }


        /// <summary>
        /// Xóa cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{objectGenCodeId}")]
        public async Task<ApiResponse> Delete([FromRoute] int objectGenCodeId)
        {
            var currentId = UserId;
            return await _objectGenCodeService.Delete(currentId, objectGenCodeId);
        }

        /// <summary>
        /// Tạo mã code
        /// </summary>
        /// <param name="objectType">Loại đối tượng cần tạo mã code</param>
        /// <returns>string Code</returns>
        [HttpGet]
        [Route("GenerateCode")]
        public async Task<ApiResponse<string>> GenerateCode([FromQuery] EnumObjectType objectType)
        {
            return await _objectGenCodeService.GenerateCode(objectType);
        }
    }
}