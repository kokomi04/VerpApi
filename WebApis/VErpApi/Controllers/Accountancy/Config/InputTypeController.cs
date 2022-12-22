﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/accountancy/config/inputType")]

    public class InputTypeConfigController : VErpBaseController
    {
        private readonly IInputPrivateConfigService _inputConfigPrivateService;

        public InputTypeConfigController(IInputPrivateConfigService inputConfigPrivateService)
        {
            _inputConfigPrivateService = inputConfigPrivateService;
        }

        [HttpGet]
        [Route("groups")]
        public async Task<IList<InputTypeGroupList>> GetList()
        {
            return await _inputConfigPrivateService.InputTypeGroupList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("groups")]
        public async Task<int> InputTypeGroupCreate([FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigPrivateService.InputTypeGroupCreate(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("groups/{inputTypeGroupId}")]
        public async Task<bool> GetInputType([FromRoute] int inputTypeGroupId, [FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigPrivateService.InputTypeGroupUpdate(inputTypeGroupId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("groups/{inputTypeGroupId}")]
        public async Task<bool> DeleteInputTypeGroup([FromRoute] int inputTypeGroupId)
        {
            return await _inputConfigPrivateService.InputTypeGroupDelete(inputTypeGroupId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<InputTypeModel>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigPrivateService.GetInputTypes(keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetAllConfig")]
        public async Task<IList<InputTypeFullModel>> GetAllConfig()
        {
            return await _inputConfigPrivateService.GetAllInputTypes().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<InputTypeSimpleModel>> GetSimpleList()
        {
            return await _inputConfigPrivateService.GetInputTypeSimpleList().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("fields")]
        public async Task<PageData<InputFieldOutputModel>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? objectApprovalStepTypeId)
        {
            return await _inputConfigPrivateService.GetInputFields(keyword, page, size, objectApprovalStepTypeId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("fields")]
        public async Task<InputFieldInputModel> AddInputField([FromBody] InputFieldInputModel inputAreaField)
        {
            return await _inputConfigPrivateService.AddInputField(inputAreaField).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("fields/{inputFieldId}")]
        public async Task<InputFieldInputModel> UpdateInputField([FromRoute] int inputFieldId, [FromBody] InputFieldInputModel inputField)
        {
            return await _inputConfigPrivateService.UpdateInputField(inputFieldId, inputField).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("fields/{inputFieldId}")]
        public async Task<bool> DeleteInputField([FromRoute] int inputFieldId)
        {
            return await _inputConfigPrivateService.DeleteInputField(inputFieldId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GlobalSetting")]
        public async Task<InputTypeGlobalSettingModel> GetInputGlobalSetting()
        {
            return await _inputConfigPrivateService.GetInputGlobalSetting().ConfigureAwait(true);
        }

        [HttpPut]
        [Route("GlobalSetting")]
        public async Task<bool> UpdateInputGlobalSetting([FromBody] InputTypeGlobalSettingModel setting)
        {
            return await _inputConfigPrivateService.UpdateInputGlobalSetting(setting).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddInputType([FromBody] InputTypeModel category)
        {
            return await _inputConfigPrivateService.AddInputType(category).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/clone")]
        public async Task<int> CloneInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigPrivateService.CloneInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        public async Task<InputTypeFullModel> GetInputTypeById([FromRoute] int inputTypeId)
        {
            return await _inputConfigPrivateService.GetInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("getByCode")]
        public async Task<InputTypeFullModel> GetInputTypeByCode([FromQuery] string inputTypeCode)
        {
            return await _inputConfigPrivateService.GetInputType(inputTypeCode).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}")]
        public async Task<bool> UpdateInputType([FromRoute] int inputTypeId, [FromBody] InputTypeModel inputType)
        {
            return await _inputConfigPrivateService.UpdateInputType(inputTypeId, inputType).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}")]
        public async Task<bool> DeleteInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigPrivateService.DeleteInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas")]
        public async Task<PageData<InputAreaModel>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigPrivateService.GetInputAreas(inputTypeId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<InputAreaModel> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigPrivateService.GetInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/basicInfo")]
        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo([FromRoute] int inputTypeId)
        {
            return await _inputConfigPrivateService.GetInputTypeBasicInfo(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<InputTypeViewModel> GetInputTypeBasicInfo([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            return await _inputConfigPrivateService.GetInputTypeViewInfo(inputTypeId, inputTypeViewId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/views")]
        public async Task<int> InputTypeViewCreate([FromRoute] int inputTypeId, [FromBody] InputTypeViewModel model)
        {
            return await _inputConfigPrivateService.InputTypeViewCreate(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId, [FromBody] InputTypeViewModel model)
        {
            if (inputTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputConfigPrivateService.InputTypeViewUpdate(inputTypeViewId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            if (inputTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputConfigPrivateService.InputTypeViewDelete(inputTypeViewId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{inputTypeId}/inputareas")]
        public async Task<int> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigPrivateService.AddInputArea(inputTypeId, inputArea).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<bool> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigPrivateService.UpdateInputArea(inputTypeId, inputAreaId, inputArea).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<bool> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigPrivateService.DeleteInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields")]
        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigPrivateService.GetInputAreaFields(inputTypeId, inputAreaId, keyword, page, size).ConfigureAwait(true);
        }

        //[HttpGet]
        //[Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaField}")]
        //public async Task<InputAreaFieldOutputFullModel> GetInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaField)
        //{
        //    return await _inputConfigPrivateService.GetInputAreaField(inputTypeId, inputAreaId, inputAreaField).ConfigureAwait(true); ;
        //}

        [HttpPost]
        [Route("{inputTypeId}/multifields")]
        public async Task<bool> UpdateMultiField([FromRoute] int inputTypeId, [FromBody] List<InputAreaFieldInputModel> fields)
        {
            return await _inputConfigPrivateService.UpdateMultiField(inputTypeId, fields).ConfigureAwait(true);
        }
    }
}
