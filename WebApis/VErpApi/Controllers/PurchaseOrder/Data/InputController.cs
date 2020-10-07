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
using VErp.Services.PurchaseOrder.Model.Input;
using VErp.Services.PurchaseOrder.Service.Input;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;

namespace VErpApi.Controllers.PurchaseOrder.Data
{

    [Route("api/PurchasingOrder/data/bills")]

    public class InputController : VErpBaseController
    {
        private readonly IInputDataService _inputDataService;

        public InputController(IInputDataService inputDataService)
        {
            _inputDataService = inputDataService;
        }


        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("{inputTypeId}/Search")]
        public async Task<PageDataTable> GetBills([FromRoute] int inputTypeId, [FromBody] InputTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.GetBills(inputTypeId, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/{fId}")]
        public async Task<PageDataTable> GetBillInfoRows([FromRoute] int inputTypeId, [FromRoute] long fId, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputDataService.GetBillInfoRows(inputTypeId, fId, orderByFieldName, asc, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/{fId}/info")]
        public async Task<BillInfoModel> GetBillInfo([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.GetBillInfo(inputTypeId, fId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{inputTypeId}")]
        public async Task<long> CreateBill([FromRoute] int inputTypeId, [FromBody] BillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.CreateBill(inputTypeId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/{fId}")]
        public async Task<bool> UpdateBill([FromRoute] int inputTypeId, [FromRoute] long fId, [FromBody] BillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.UpdateBill(inputTypeId, fId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/multiple")]
        public async Task<bool> UpdateMultipleBills([FromRoute] int inputTypeId, [FromBody] UpdateMultipleModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _inputDataService.UpdateMultipleBills(inputTypeId, data.FieldName, data.OldValue, data.NewValue, data.FIds).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/{fId}")]
        public async Task<bool> DeleteBill([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.DeleteBill(inputTypeId, fId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/importFromMapping")]
        public async Task<bool> ImportFromMapping([FromRoute] int inputTypeId, [FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputDataService.ImportBillFromMapping(inputTypeId, JsonConvert.DeserializeObject<ImportBillExelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/{fId}/datafile")]
        public async Task<FileStreamResult> ExportCategoryRow([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            var result = await _inputDataService.ExportBill(inputTypeId, fId);
            return new FileStreamResult(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = result.FileName };
        }
    }
}
