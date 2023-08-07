using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Service.Input.Implement.Facade;

namespace VErpApi.Controllers.Accountancy.Data
{

    [Route("api/accountancy/data/bills")]
    [ObjectDataApi(EnumObjectType.InputType, "inputTypeId")]
    public class InputController : InputControllerBaseAbstract
    {

        public InputController(IInputDataPrivateService inputDataService, IInpuDataExportFacadeService inpuDataExportFacadeService)
            : base(inputDataService, inpuDataExportFacadeService)
        {

        }

    }

    [Route("api/accountancy/public/bills")]
    [ObjectDataApi(EnumObjectType.InputTypePublic, "inputTypeId")]
    public class InputPublicController : InputControllerBaseAbstract
    {
        public InputPublicController(IInputDataPublicService inputDataService, IInpuDataExportFacadeService inpuDataExportFacadeService)
            : base(inputDataService, inpuDataExportFacadeService)
        {

        }

    }


    public abstract class InputControllerBaseAbstract : VErpBaseController
    {
        private readonly IInputDataServiceBase _inputDataService;
        private readonly IInpuDataExportFacadeService _inpuDataExportFacadeService;

        public InputControllerBaseAbstract(IInputDataServiceBase inputDataService, IInpuDataExportFacadeService inpuDataExportFacadeService)
        {
            _inputDataService = inputDataService;
            _inpuDataExportFacadeService = inpuDataExportFacadeService;
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{inputTypeId}/Search")]
        public async Task<PageDataTable> GetBills([FromRoute] int inputTypeId, [FromBody] InputTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.GetBills(inputTypeId, request.IsMultirow, request.FromDate, request.ToDate, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{inputTypeId}/Export")]
        public async Task<IActionResult> ExportList([FromRoute] int inputTypeId, [FromBody] InputTypeBillsExportFilterModel req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            var (stream, fileName, contentType) = await _inpuDataExportFacadeService.Export(_inputDataService, inputTypeId, req);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }


        [HttpGet]
        [Route("{inputTypeId}/{fId}")]
        public async Task<PageDataTable> GetBillInfoRows([FromRoute] int inputTypeId, [FromRoute] long fId, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromQuery] int? page, [FromQuery] int? size)
        {
            return await _inputDataService.GetBillInfoRows(inputTypeId, fId, orderByFieldName, asc, page ?? 1, size ?? 0).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{inputTypeId}/getByListIds")]
        public async Task<IDictionary<long, BillInfoModel>> GetListBillInfoRows([FromRoute] int inputTypeId, [FromBody] IList<long> fIds)
        {
            return await _inputDataService.GetBillInfos(inputTypeId, fIds).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/{fId}/info")]
        public async Task<BillInfoModel> GetBillInfo([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.GetBillInfo(inputTypeId, fId).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{inputTypeId}/infoByParent")]
        public async Task<BillInfoModel> GetBillInfoByParent([FromRoute] int inputTypeId, [FromQuery] long parentId)
        {
            return await _inputDataService.GetBillInfoByParent(inputTypeId, parentId).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{inputTypeId}/{fId}/AllocationDataBillCodes")]
        public async Task<IList<string>> GetAllocationDataBillCodes([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            return await _inputDataService.GetAllocationDataBillCodes(inputTypeId, fId).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/{fId}/AllocationDataBillCodes")]
        public async Task<bool> UpdateAllocationDataBillCodes([FromRoute] int inputTypeId, [FromRoute] long fId, [FromBody] IList<string> dataAllowcationBillCodes)
        {
            return await _inputDataService.UpdateAllocationDataBillCodes(inputTypeId, fId, dataAllowcationBillCodes).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/{fId}/CalcResultAllowcation")]
        public async Task<IList<NonCamelCaseDictionary>> CalcResultAllowcation(int inputTypeId, long fId)
        {
            return await _inputDataService.CalcResultAllowcation(inputTypeId, fId);
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
        public async Task<bool> UpdateBill([FromRoute] int inputTypeId, [FromRoute] long fId, [FromBody] BillInfoModel data, [FromQuery] bool isDeleteAllowcationBill)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputDataService.UpdateBill(inputTypeId, fId, data, isDeleteAllowcationBill).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/multiple")]
        public async Task<bool> UpdateMultipleBills([FromRoute] int inputTypeId, [FromBody] UpdateMultipleModel data, [FromQuery] bool isDeleteAllowcationBill)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _inputDataService.UpdateMultipleBills(inputTypeId, data.FieldName, data.OldValue, data.NewValue, data.BillIds, data.DetailIds, isDeleteAllowcationBill).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/{fId}")]
        public async Task<bool> DeleteBill([FromRoute] int inputTypeId, [FromRoute] long fId, [FromQuery] bool isDeleteAllowcationBill)
        {
            return await _inputDataService.DeleteBill(inputTypeId, fId, isDeleteAllowcationBill).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping([FromRoute] int inputTypeId, [FromQuery] int? areaId, [FromQuery] bool? isExport)
        {
            return await _inputDataService.GetFieldDataForMapping(inputTypeId, areaId, isExport);
        }

        [HttpPost]
        [Route("{inputTypeId}/importFromMapping")]
        public async Task<bool> ImportFromMapping([FromRoute] int inputTypeId, [FromFormString] ImportExcelMapping mapping, IFormFile file, [FromQuery] bool isDeleteAllowcationBill)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputDataService.ImportBillFromMapping(inputTypeId, mapping, file.OpenReadStream(), isDeleteAllowcationBill).ConfigureAwait(true);
        }



        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{inputTypeId}/parseExcelFromMapping")]
        public async Task<BillInfoModel> ParseBillFromMapping([FromRoute] int inputTypeId, [FromFormString] BillParseMapping parseMapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputDataService.ParseBillFromMapping(inputTypeId, parseMapping, file.OpenReadStream()).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{inputTypeId}/{fId}/datafile")]
        public async Task<FileStreamResult> ExportBill([FromRoute] int inputTypeId, [FromRoute] long fId)
        {
            var result = await _inputDataService.ExportBill(inputTypeId, fId);
            return new FileStreamResult(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = result.FileName };
        }

        [HttpGet]
        [Route("{inputTypeId}/GetBillNotApprovedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet([FromRoute] int inputTypeId)
        {
            return await _inputDataService.GetBillNotApprovedYet(inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}/GetBillNotChekedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet([FromRoute] int inputTypeId)
        {
            return await _inputDataService.GetBillNotChekedYet(inputTypeId);
        }

        [HttpPut]
        [Route("{inputTypeId}/CheckAllBillInList")]
        public async Task<bool> CheckAllBillInList([FromRoute] int inputTypeId, [FromBody] IList<ObjectBillSimpleInfoModel> models)
        {
            return await _inputDataService.CheckAllBillInList(models);
        }

        [HttpPut]
        [Route("{inputTypeId}/ApproveAllBillInList")]
        public async Task<bool> ApproveAllBillInList([FromRoute] int inputTypeId, [FromBody] IList<ObjectBillSimpleInfoModel> models)
        {
            return await _inputDataService.ApproveAllBillInList(models);
        }

    }
}
