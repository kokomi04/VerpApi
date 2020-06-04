﻿using System.Threading.Tasks;
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
        private readonly IInputTypeService _inputTypeService;
        private readonly IInputAreaService _inputAreaService;
        private readonly IInputAreaFieldService _inputAreaFieldService;
        private readonly IInputValueBillService _inputValueBillService;
        private readonly IFileService _fileService;
        public InputController(IInputTypeService inputTypeService
            , IInputAreaService inputAreaService
            , IInputAreaFieldService inputAreaFieldService
            , IInputValueBillService inputValueBillService
            , IFileService fileService
            )
        {
            _fileService = fileService;
            _inputTypeService = inputTypeService;
            _inputAreaService = inputAreaService;
            _inputAreaFieldService = inputAreaFieldService;
            _inputValueBillService = inputValueBillService;
        }


        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<InputTypeModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputTypeService.GetInputTypes(keyword, page, size);
        }

        [HttpGet]
        [Route("fields")]
        public async Task<ServiceResult<PageData<InputAreaFieldOutputFullModel>>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputAreaFieldService.GetAll(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddInputType([FromBody] InputTypeModel category)
        {
            return await _inputTypeService.AddInputType(category);
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult<InputTypeFullModel>> GetInputType([FromRoute] int inputTypeId)
        {
            return await _inputTypeService.GetInputType(inputTypeId);
        }

        [HttpPut]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult> UpdateInputType([FromRoute] int inputTypeId, [FromBody] InputTypeModel inputType)
        {
            return await _inputTypeService.UpdateInputType(inputTypeId, inputType);
        }

        [HttpDelete]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult> DeleteInputType([FromRoute] int inputTypeId)
        {
            return await _inputTypeService.DeleteInputType(inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<PageData<InputAreaModel>>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputAreaService.GetInputAreas(inputTypeId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult<InputAreaModel>> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputAreaService.GetInputArea(inputTypeId, inputAreaId);
        }

        [HttpGet]
        [Route("{inputTypeId}/basicInfo")]
        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo([FromRoute] int inputTypeId)
        {
            return await _inputTypeService.GetInputTypeBasicInfo(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<InputTypeViewModel> GetInputTypeBasicInfo([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            return await _inputTypeService.GetInputTypeViewInfo(inputTypeId, inputTypeViewId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/views")]
        public async Task<int> InputTypeViewCreate([FromRoute] int inputTypeId, [FromBody] InputTypeViewModel model)
        {
            return await _inputTypeService.InputTypeViewCreate(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId, [FromBody] InputTypeViewModel model)
        {
            var r = await _inputTypeService.InputTypeViewUpdate(inputTypeViewId, model).ConfigureAwait(true);
            return r.IsSuccess();
        }

        [HttpDelete]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            var r = await _inputTypeService.InputTypeViewDelete(inputTypeViewId).ConfigureAwait(true);
            return r.IsSuccess();
        }


        [HttpPost]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<int>> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputAreaService.AddInputArea(inputTypeId, inputArea);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputAreaService.UpdateInputArea(inputTypeId, inputAreaId, inputArea);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputAreaService.DeleteInputArea(inputTypeId, inputAreaId);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields")]
        public async Task<ServiceResult<PageData<InputAreaFieldOutputFullModel>>> GetInputAreaFields([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputAreaFieldService.GetInputAreaFields(inputTypeId, inputAreaId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaField}")]
        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaField)
        {
            return await _inputAreaFieldService.GetInputAreaField(inputTypeId, inputAreaId, inputAreaField);
        }

        [HttpPost]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields")]
        public async Task<ServiceResult<int>> AddInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaFieldInputModel inputAreaField)
        {
            return await _inputAreaFieldService.AddInputAreaField(inputTypeId, inputAreaId, inputAreaField);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaFieldId}")]
        public async Task<ServiceResult> UpdateInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaFieldId, [FromBody] InputAreaFieldInputModel inputAreaField)
        {
            return await _inputAreaFieldService.UpdateInputAreaField(inputTypeId, inputAreaId, inputAreaFieldId, inputAreaField);
        }

        [HttpPost]
        [Route("{inputTypeId}/multifields")]
        public async Task<ServiceResult> UpdateMultiField([FromRoute] int inputTypeId, [FromBody] List<InputAreaFieldInputModel> fields)
        {
            return await _inputAreaFieldService.UpdateMultiField(inputTypeId, fields);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaFieldId}")]
        public async Task<ServiceResult> DeleteInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaFieldId)
        {
            return await _inputAreaFieldService.DeleteInputAreaField(inputTypeId, inputAreaId, inputAreaFieldId);
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