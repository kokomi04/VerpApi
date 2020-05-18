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
        public async Task<ServiceResult<PageData<CustomGenCodeOutputModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customGenCodeService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Lấy thông tin cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="customGenCodeId">Id cấu hình</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{customGenCodeId}")]
        public async Task<ServiceResult<CustomGenCodeOutputModel>> GetInfo([FromRoute] int customGenCodeId)
        {
            return await _customGenCodeService.GetInfo(customGenCodeId);
        }

        [HttpGet]
        [Route("currentConfig")]
        public async Task<ServiceResult<CustomGenCodeOutputModel>> GetCurrentConfig([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _customGenCodeService.GetCurrentConfig(objectTypeId, objectId);
        }

        /// <summary>
        /// Thêm mới cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> Post([FromBody] CustomGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _customGenCodeService.Create(currentId, req);
        }

        /// <summary>
        /// Cập nhật cấu hình gen code
        /// </summary>
        /// <param name="customGenCodeId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{customGenCodeId}")]
        public async Task<ServiceResult> Update([FromRoute] int customGenCodeId, [FromBody] CustomGenCodeInputModel req)
        {
            var currentId = UserId;
            return await _customGenCodeService.Update(customGenCodeId, currentId, req);
        }

        /// <summary>
        /// Mapping cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("objectCustomGenCode")]
        public async Task<ServiceResult> MapObjectCustomGenCode([FromBody] ObjectCustomGenCodeMapping req)
        {
            var currentId = UserId;
            return await _customGenCodeService.MapObjectCustomGenCode(currentId, req);
        }

        /// <summary>
        /// Xóa cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="customGenCodeId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{customGenCodeId}")]
        public async Task<ServiceResult> Delete([FromRoute] int customGenCodeId)
        {
            var currentId = UserId;
            return await _customGenCodeService.Delete(currentId, customGenCodeId);
        }

        /// <summary>
        /// Tạo mã code
        /// </summary>
        /// <param name="customGenCodeId">Id cấu hình gen code</param>
        /// <returns>string Code</returns>
        [HttpGet]
        [Route("generateCode")]
        public async Task<ServiceResult<CustomCodeModel>> GenerateCode([FromQuery] int customGenCodeId, [FromQuery] int lastValue)
        {
            return await _customGenCodeService.GenerateCode(customGenCodeId, lastValue);
        }

        [HttpPut]
        [Route("confirmCode")]
        public async Task<ServiceResult> ConfirmCode([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _customGenCodeService.ConfirmCode(objectTypeId, objectId);
        }

        /// <summary>
        /// Lấy danh sách các loại đối tượng 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAllObjectType")]
        public async Task<ServiceResult<PageData<ObjectType>>> GetAllObjectType()
        {
            return await _customGenCodeService.GetAllObjectType();
        }
    }
}