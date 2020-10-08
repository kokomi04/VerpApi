using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;

namespace VErpApi.Controllers.PurchaseOrder.Data
{

    [Route("api/PurchasingOrder/data/salebills")]

    public class InputController : VErpBaseController
    {
        private readonly IVoucherDataService _inputDataService;

        public InputController(IVoucherDataService inputDataService)
        {
            _inputDataService = inputDataService;
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("{voucherTypeId}/Search")]
        public async Task<PageDataTable> GetSaleBills([FromRoute] int voucherTypeId, [FromBody] VoucherTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.GetSaleBills(voucherTypeId, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<PageDataTable> GetSaleBillInfoRows([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputDataService.GetSaleBillInfoRows(voucherTypeId, fId, orderByFieldName, asc, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}/info")]
        public async Task<SaleBillInfoModel> GetSaleBillInfo([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.GetSaleBillInfo(voucherTypeId, fId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{voucherTypeId}")]
        public async Task<long> CreateSaleBill([FromRoute] int voucherTypeId, [FromBody] SaleBillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.CreateSaleBill(voucherTypeId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<bool> UpdateSaleBill([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromBody] SaleBillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.UpdateSaleBill(voucherTypeId, fId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/multiple")]
        public async Task<bool> UpdateMultipleSaleBills([FromRoute] int voucherTypeId, [FromBody] UpdateMultipleModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _inputDataService.UpdateMultipleSaleBills(voucherTypeId, data.FieldName, data.OldValue, data.NewValue, data.FIds).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<bool> DeleteSaleBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.DeleteSaleBill(voucherTypeId, fId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/importFromMapping")]
        public async Task<bool> ImportSaleBillFromMapping([FromRoute] int voucherTypeId, [FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputDataService.ImportSaleBillFromMapping(voucherTypeId, JsonConvert.DeserializeObject<ImportBillExelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}/datafile")]
        public async Task<FileStreamResult> ExportSaleBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            var result = await _inputDataService.ExportSaleBill(voucherTypeId, fId);
            return new FileStreamResult(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = result.FileName };
        }
    }
}
