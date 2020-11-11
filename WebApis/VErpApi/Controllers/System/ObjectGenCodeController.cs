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

    [Route("api/GenCodeConfigs")]
    public class ObjectGenCodeController : VErpBaseController
    {
        private readonly IObjectGenCodeService _customGenCodeService;
        public ObjectGenCodeController(IObjectGenCodeService objectGenCodeService
            )
        {
            _customGenCodeService = objectGenCodeService;
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
        public async Task<PageData<ObjectGenCodeModel>> Get([FromQuery] EnumObjectType objectType, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _customGenCodeService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Lấy thông tin cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId">Id cấu hình</param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("{objectGenCodeId}")]
        //public async Task<ObjectGenCodeOutputModel> GetInfo([FromRoute] int objectGenCodeId)
        //{
        //    return await _customGenCodeService.GetInfo(objectGenCodeId);
        //}


        /// <summary>
        /// Thêm mới cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectType">Loại đối tượng</param>
        /// <param name="req"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("")]
        //public async Task<int> Post([FromQuery] EnumObjectType objectType, [FromBody] ObjectGenCodeInputModel req)
        //{
        //    var currentId = UserId;
        //    return await _customGenCodeService.Create(objectType, currentId, req);
        //}


        /// <summary>
        /// Cập nhật cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        //[HttpPut]
        //[Route("{objectGenCodeId}")]
        //public async Task<bool> Update([FromRoute] int objectGenCodeId, [FromBody] ObjectGenCodeInputModel req)
        //{
        //    var currentId = UserId;
        //    return await _customGenCodeService.Update(objectGenCodeId, currentId,req);
        //}


        /// <summary>
        /// Xóa cấu hình gen code cho đối tượng
        /// </summary>
        /// <param name="objectGenCodeId"></param>
        /// <returns></returns>
        //[HttpDelete]
        //[Route("{objectGenCodeId}")]
        //public async Task<bool> Delete([FromRoute] int objectGenCodeId)
        //{
        //    var currentId = UserId;
        //    return await _customGenCodeService.Delete(currentId, objectGenCodeId);
        //}

        /// <summary>
        /// Tạo mã code
        /// </summary>
        /// <param name="objectType">Loại đối tượng cần tạo mã code</param>
        /// <returns>string Code</returns>
        [HttpGet]
        [Route("GenerateCode")]
        public async Task<string> GenerateCode([FromQuery] EnumObjectType objectType)
        {
            return await _customGenCodeService.GenerateCode(objectType);
        }

        /// <summary>
        /// Lấy danh sách các loại đối tượng 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllObjectType")]
        public PageData<ObjectType> GetAllObjectType()
        {
            return _customGenCodeService.GetAllObjectType();
        }

        [HttpPost]
        [Route("MapObjectGenCode")]
        public async Task<bool> MapObjectGenCode(ObjectGenCodeMapping model)
        {
            return await _customGenCodeService.MapObjectGenCode(model);
        }

        [HttpDelete]
        [Route("MapObjectGenCode/{ObjectCustomGenCodeMappingId}")]
        public async Task<bool> DeleteMapObjectGenCode([FromRoute] int objectCustomGenCodeMappingId)
        {
            return await _customGenCodeService.DeleteMapObjectGenCode(objectCustomGenCodeMappingId);
        }
    }
}