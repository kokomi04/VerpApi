using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Accountant.Model.Input;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using VErp.Services.Accountant.Service.Input;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.GlobalObject;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/inputtypes")]

    public class InputController : VErpBaseController
    {
        private readonly IInputConfigService _inputConfigService;
        private readonly IInputValueBillService _inputValueBillService;
        private readonly IFileService _fileService;
        public InputController(IInputConfigService inputConfigService
            , IInputValueBillService inputValueBillService
            , IFileService fileService
            )
        {
            _fileService = fileService;
            _inputConfigService = inputConfigService;
            _inputValueBillService = inputValueBillService;
        }


        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<InputTypeModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigService.GetInputTypes(keyword, page, size);
        }

        [HttpGet]
        [Route("fields")]
        public async Task<ServiceResult<PageData<InputFieldOutputModel>>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigService.GetInputFields(keyword, page, size);
        }

        [HttpPost]
        [Route("fields")]
        public async Task<ServiceResult<int>> AddInputField([FromBody] InputFieldInputModel inputAreaField)
        {
            return await _inputConfigService.AddInputField(inputAreaField);
        }

        [HttpPut]
        [Route("fields/{inputFieldId}")]
        public async Task<ServiceResult> UpdateInputField([FromRoute] int inputFieldId, [FromBody] InputFieldInputModel inputField)
        {
            return await _inputConfigService.UpdateInputField(inputFieldId, inputField);
        }

        [HttpDelete]
        [Route("fields/{inputFieldId}")]
        public async Task<ServiceResult> DeleteInputField([FromRoute] int inputFieldId)
        {
            return await _inputConfigService.DeleteInputField(inputFieldId);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddInputType([FromBody] InputTypeModel category)
        {
            return await _inputConfigService.AddInputType(category);
        }

        [HttpPost]
        [Route("clone")]
        public async Task<ServiceResult<int>> CloneInputType([FromBody] int inputTypeId)
        {
            return await _inputConfigService.CloneInputType(inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult<InputTypeFullModel>> GetInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigService.GetInputType(inputTypeId);
        }

        [HttpPut]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult> UpdateInputType([FromRoute] int inputTypeId, [FromBody] InputTypeModel inputType)
        {
            return await _inputConfigService.UpdateInputType(inputTypeId, inputType);
        }

        [HttpDelete]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult> DeleteInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigService.DeleteInputType(inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<PageData<InputAreaModel>>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigService.GetInputAreas(inputTypeId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult<InputAreaModel>> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigService.GetInputArea(inputTypeId, inputAreaId);
        }

        [HttpGet]
        [Route("{inputTypeId}/basicInfo")]
        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo([FromRoute] int inputTypeId)
        {
            return await _inputConfigService.GetInputTypeBasicInfo(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<InputTypeViewModel> GetInputTypeBasicInfo([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            return await _inputConfigService.GetInputTypeViewInfo(inputTypeId, inputTypeViewId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/views")]
        public async Task<int> InputTypeViewCreate([FromRoute] int inputTypeId, [FromBody] InputTypeViewModel model)
        {
            return await _inputConfigService.InputTypeViewCreate(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId, [FromBody] InputTypeViewModel model)
        {
            var r = await _inputConfigService.InputTypeViewUpdate(inputTypeViewId, model).ConfigureAwait(true);
            return r.IsSuccess();
        }

        [HttpDelete]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            var r = await _inputConfigService.InputTypeViewDelete(inputTypeViewId).ConfigureAwait(true);
            return r.IsSuccess();
        }


        [HttpPost]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<int>> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigService.AddInputArea(inputTypeId, inputArea);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigService.UpdateInputArea(inputTypeId, inputAreaId, inputArea);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigService.DeleteInputArea(inputTypeId, inputAreaId);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields")]
        public async Task<ServiceResult<PageData<InputAreaFieldOutputFullModel>>> GetInputAreaFields([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigService.GetInputAreaFields(inputTypeId, inputAreaId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaField}")]
        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaField)
        {
            return await _inputConfigService.GetInputAreaField(inputTypeId, inputAreaId, inputAreaField);
        }

        [HttpPost]
        [Route("{inputTypeId}/multifields")]
        public async Task<ServiceResult> UpdateMultiField([FromRoute] int inputTypeId, [FromBody] List<InputAreaFieldInputModel> fields)
        {
            return await _inputConfigService.UpdateMultiField(inputTypeId, fields);
        }

        [HttpGet]
        [Route("{inputTypeId}/listInfo")]
        public async Task<ServiceResult<InputTypeListInfo>> GetInputValueBills([FromRoute] int inputTypeId)
        {
            return await _inputValueBillService.GetInputTypeListInfo(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/bills")]
        public async Task<ServiceResult<PageData<InputValueBillListOutput>>> GetInputValueBills([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] IList<InputValueFilterModel> fieldFilters, [FromQuery] string orderBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputValueBillService.GetInputValueBills(inputTypeId, keyword, fieldFilters, orderBy, asc, page, size).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("{inputTypeId}/bills")]
        public async Task<ServiceResult<PageData<InputValueBillListOutput>>> GetInputTypeBills([FromRoute] int inputTypeId, [FromBody] InputTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _inputValueBillService.GetInputValueBills(inputTypeId, request.Keyword, request.FieldFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputvaluebills/{inputValueBillId}")]
        public async Task<ServiceResult<InputValueOuputModel>> GetInputValueBill([FromRoute] int inputTypeId, [FromRoute] long inputValueBillId)
        {
            return await _inputValueBillService.GetInputValueBill(inputTypeId, inputValueBillId);
        }

        [HttpPost]
        [Route("{inputTypeId}/inputvaluebills")]
        public async Task<ServiceResult<long>> AddInputValueBill([FromRoute] int inputTypeId, [FromBody] InputValueInputModel data)
        {
            return await _inputValueBillService.AddInputValueBill(inputTypeId, data);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputvaluebills/{inputValueBillId}")]
        public async Task<ServiceResult<long>> UpdateInputValueBill([FromRoute] int inputTypeId, [FromRoute] long inputValueBillId, [FromBody] InputValueInputModel data)
        {
            return await _inputValueBillService.UpdateInputValueBill(inputTypeId, inputValueBillId, data);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputvaluebills/{inputValueBillId}")]
        public async Task<ServiceResult> DeleteInputValueBill([FromRoute] int inputTypeId, [FromRoute] long inputValueBillId)
        {
            return await _inputValueBillService.DeleteInputValueBill(inputTypeId, inputValueBillId);
        }
    }
}