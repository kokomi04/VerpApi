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

    [Route("api/barcodeConfigs")]
    public class BarcodeConfigController : VErpBaseController
    {
        private readonly IBarcodeConfigService _barcodeConfigService;
        public BarcodeConfigController(IBarcodeConfigService barcodeConfigService
            )
        {
            _barcodeConfigService = barcodeConfigService;
        }

        /// <summary>
        /// Tìm kiếm cấu hình barcode
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<BarcodeConfigListOutput>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _barcodeConfigService.GetList(keyword, page, size);
        }
       

        /// <summary>
        /// Thêm mới cấu hình barcode
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<int> Post([FromBody] BarcodeConfigModel req)
        {
            return await _barcodeConfigService.AddBarcodeConfig(req);
        }



        /// <summary>
        /// Lấy thông tin barcode
        /// </summary>
        /// <param name="barcodeConfigId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{barcodeConfigId}")]
        public async Task<BarcodeConfigModel> Info([FromRoute] int barcodeConfigId)
        {
            return await _barcodeConfigService.GetInfo(barcodeConfigId);
        }


        /// <summary>
        /// Cập nhật cấu hình barcode
        /// </summary>
        /// <param name="barcodeConfigId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{barcodeConfigId}")]
        public async Task<bool> Update([FromRoute] int barcodeConfigId, [FromBody] BarcodeConfigModel req)
        {
            return await _barcodeConfigService.UpdateBarcodeConfig(barcodeConfigId, req);
        }


        /// <summary>
        /// Xóa cấu hình barcode
        /// </summary>
        /// <param name="barcodeConfigId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{barcodeConfigId}")]
        public async Task<bool> Delete([FromRoute] int barcodeConfigId)
        {
            return await _barcodeConfigService.DeleteBarcodeConfig(barcodeConfigId);
        }

        /// <summary>
        /// Tạo mã barcode
        /// </summary>
        /// <param name="barcodeConfigId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{barcodeConfigId}/Generate")]
        public async Task<string> Make([FromRoute] int barcodeConfigId)
        {
            return await _barcodeConfigService.Make(barcodeConfigId);
        }
    }
}