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

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddInputType([FromBody] InputTypeModel category)
        {
            var updatedUserId = UserId;
            return await _inputTypeService.AddInputType(updatedUserId, category);
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
            var updatedUserId = UserId;
            return await _inputTypeService.UpdateInputType(updatedUserId, inputTypeId, inputType);
        }

        [HttpDelete]
        [Route("{inputTypeId}")]
        public async Task<ServiceResult> DeleteInputType([FromRoute] int inputTypeId)
        {
            var updatedUserId = UserId;
            return await _inputTypeService.DeleteInputType(updatedUserId, inputTypeId);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<PageData<InputAreaOutputModel>>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputAreaService.GetInputAreas(inputTypeId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult<InputAreaOutputModel>> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputAreaService.GetInputArea(inputTypeId, inputAreaId);
        }

        [HttpPost]
        [Route("{inputTypeId}/inputareas")]
        public async Task<ServiceResult<int>> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
        {
            var updatedUserId = UserId;
            return await _inputAreaService.AddInputArea(updatedUserId, inputTypeId, inputArea);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
        {
            var updatedUserId = UserId;
            return await _inputAreaService.UpdateInputArea(updatedUserId, inputTypeId, inputAreaId, inputArea);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<ServiceResult> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            var updatedUserId = UserId;
            return await _inputAreaService.DeleteInputArea(updatedUserId, inputTypeId, inputAreaId);
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
            var updatedUserId = UserId;
            return await _inputAreaFieldService.AddInputAreaField(updatedUserId, inputTypeId, inputAreaId, inputAreaField);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaFieldId}")]
        public async Task<ServiceResult> UpdateInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaFieldId, [FromBody] InputAreaFieldInputModel inputAreaField)
        {
            var updatedUserId = UserId;
            return await _inputAreaFieldService.UpdateInputAreaField(updatedUserId, inputTypeId, inputAreaId, inputAreaFieldId, inputAreaField);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaFieldId}")]
        public async Task<ServiceResult> DeleteInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaFieldId)
        {
            var updatedUserId = UserId;
            return await _inputAreaFieldService.DeleteInputAreaField(updatedUserId, inputTypeId, inputAreaId, inputAreaFieldId);
        }

        [HttpGet]
        [Route("{inputTypeId}/listInfo")]
        public async Task<ServiceResult<InputTypeListInfo>> GetInputValueBills([FromRoute] int inputTypeId)
        {
            return await _inputValueBillService.GetInputTypeListInfo(inputTypeId).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{inputTypeId}/bills")]
        public async Task<ServiceResult<PageData<InputValueBillListOutput>>> GetInputValueBills([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] IList<InputValueFilterModel> fieldFilters, [FromQuery] int orderByFieldId, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputValueBillService.GetInputValueBills(inputTypeId, keyword, fieldFilters, orderByFieldId, asc, page, size).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputvaluebills")]
        public async Task<ServiceResult<PageData<InputValueBillOutputModel>>> GetInputValueBills([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputValueBillService.GetInputValueBills(inputTypeId, keyword, page, size);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputvaluebills/{inputValueBillId}")]
        public async Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill([FromRoute] int inputTypeId, [FromRoute] long inputValueBillId)
        {
            return await _inputValueBillService.GetInputValueBill(inputTypeId, inputValueBillId);
        }

        [HttpPost]
        [Route("{inputTypeId}/inputvaluebills")]
        public async Task<ServiceResult<long>> AddInputValueBill([FromRoute] int inputTypeId, [FromBody] InputValueBillInputModel data)
        {
            var updatedUserId = UserId;
            return await _inputValueBillService.AddInputValueBill(updatedUserId, inputTypeId, data);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputvaluebills/{inputValueBillId}")]
        public async Task<ServiceResult> DeleteInputValueBill([FromRoute] int inputTypeId, [FromRoute] long inputValueBillId)
        {
            var updatedUserId = UserId;
            return await _inputValueBillService.DeleteInputValueBill(updatedUserId, inputTypeId, inputValueBillId);
        }
    }
}