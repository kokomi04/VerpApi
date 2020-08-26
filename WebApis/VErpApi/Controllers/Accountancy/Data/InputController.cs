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
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;

namespace VErpApi.Controllers.Accountancy.Data
{

    [Route("api/accountancy/data/bills")]

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
        [GlobalApi]
        [Route("GetBillInfoByMappingObject")]
        public async Task<PageDataTable> GetBillInfoByMappingObject([FromQuery] string mappingFunctionKey, [FromQuery] string objectId)
        {
            return await _inputDataService.GetBillInfoByMappingObject(mappingFunctionKey, objectId).ConfigureAwait(true);
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

        [HttpGet]
        [Route("CalcFixExchangeRate")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate([FromQuery] long toDate, [FromQuery] int currency, [FromQuery] int exchangeRate)
        {
            return await _inputDataService.CalcFixExchangeRate(toDate, currency, exchangeRate);
        }

        [HttpGet]
        [Route("CalcCostTransfer")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer([FromQuery] long toDate, [FromQuery] EnumCostTransfer type, [FromQuery] bool byDepartment,
            [FromQuery] bool byCustomer, [FromQuery] bool byFixedAsset, [FromQuery] bool byExpenseItem, [FromQuery] bool byFactory, [FromQuery] bool byProduct, [FromQuery] bool byStock)
        {
            return await _inputDataService.CalcCostTransfer(toDate, type, byDepartment, byCustomer, byFixedAsset, byExpenseItem, byFactory, byProduct, byStock);
        }

        [HttpGet]
        [Route("CostTransferType")]
        public ICollection<CostTransferTypeModel> GetCostTransferTypes()
        {
            return _inputDataService.GetCostTransferTypes();
        }

        [HttpGet]
        [Route("CheckExistedFixExchangeRate")]
        public async Task<bool> CheckExistedFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.CheckExistedFixExchangeRate(fromDate, toDate);
        }

        [HttpDelete]
        [Route("DeletedFixExchangeRate")]
        public async Task<bool> DeletedFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.DeletedFixExchangeRate(fromDate, toDate);
        }

        [HttpGet]
        [Route("CheckExistedCostTransfer")]
        public async Task<bool> CheckExistedCostTransfer([FromQuery] EnumCostTransfer type, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.CheckExistedCostTransfer(type, fromDate, toDate);
        }

        [HttpDelete]
        [Route("DeletedCostTransfer")]
        public async Task<bool> DeletedCostTransfer([FromQuery] EnumCostTransfer type, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.DeletedCostTransfer(type, fromDate, toDate);
        }

        [HttpGet]
        [Route("CalcCostTransferBalanceZero")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero([FromQuery] long toDate)
        {
            return await _inputDataService.CalcCostTransferBalanceZero(toDate);
        }

        [HttpGet]
        [Route("CheckExistedCostTransferBalanceZero")]
        public async Task<bool> CheckExistedCostTransferBalanceZero([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.CheckExistedCostTransferBalanceZero(fromDate, toDate);
        }

        [HttpDelete]
        [Route("DeletedCostTransferBalanceZero")]
        public async Task<bool> DeletedCostTransferBalanceZero([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _inputDataService.DeletedCostTransferBalanceZero(fromDate, toDate);
        }
    }
}
