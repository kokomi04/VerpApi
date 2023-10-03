﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Config
{
    public abstract class InputTypeConfigControllerAbstract : VErpBaseController
    {
        private readonly IInputConfigServiceBase _inputConfigServiceBase;

        public InputTypeConfigControllerAbstract(IInputConfigServiceBase inputConfigServiceBase)
        {
            _inputConfigServiceBase = inputConfigServiceBase;
        }

        [HttpGet]
        [Route("groups")]
        public async Task<IList<InputTypeGroupList>> GetList()
        {
            return await _inputConfigServiceBase.InputTypeGroupList().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("groups")]
        public async Task<int> InputTypeGroupCreate([FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigServiceBase.InputTypeGroupCreate(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("groups/{inputTypeGroupId}")]
        public async Task<bool> GetInputType([FromRoute] int inputTypeGroupId, [FromBody] InputTypeGroupModel model)
        {
            return await _inputConfigServiceBase.InputTypeGroupUpdate(inputTypeGroupId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("groups/{inputTypeGroupId}")]
        public async Task<bool> DeleteInputTypeGroup([FromRoute] int inputTypeGroupId)
        {
            return await _inputConfigServiceBase.InputTypeGroupDelete(inputTypeGroupId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<InputTypeModel>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigServiceBase.GetInputTypes(keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetAllConfig")]
        public async Task<IList<InputTypeFullModel>> GetAllConfig()
        {
            return await _inputConfigServiceBase.GetAllInputTypes().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<InputTypeSimpleModel>> GetSimpleList()
        {
            return await _inputConfigServiceBase.GetInputTypeSimpleList().ConfigureAwait(true);
        }

        [HttpGet]
        [Route("fields")]
        public async Task<PageData<InputFieldOutputModel>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? objectApprovalStepTypeId)
        {
            return await _inputConfigServiceBase.GetInputFields(keyword, page, size, objectApprovalStepTypeId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("fields")]
        public async Task<InputFieldInputModel> AddInputField([FromBody] InputFieldInputModel inputAreaField)
        {
            return await _inputConfigServiceBase.AddInputField(inputAreaField).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("fields/{inputFieldId}")]
        public async Task<InputFieldInputModel> UpdateInputField([FromRoute] int inputFieldId, [FromBody] InputFieldInputModel inputField)
        {
            return await _inputConfigServiceBase.UpdateInputField(inputFieldId, inputField).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("fields/{inputFieldId}")]
        public async Task<bool> DeleteInputField([FromRoute] int inputFieldId)
        {
            return await _inputConfigServiceBase.DeleteInputField(inputFieldId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GlobalSetting")]
        public async Task<InputTypeGlobalSettingModel> GetInputGlobalSetting()
        {
            return await _inputConfigServiceBase.GetInputGlobalSetting().ConfigureAwait(true);
        }

        [HttpPut]
        [Route("GlobalSetting")]
        public async Task<bool> UpdateInputGlobalSetting([FromBody] InputTypeGlobalSettingModel setting)
        {
            return await _inputConfigServiceBase.UpdateInputGlobalSetting(setting).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddInputType([FromBody] InputTypeModel category)
        {
            return await _inputConfigServiceBase.AddInputType(category).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/clone")]
        public async Task<int> CloneInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigServiceBase.CloneInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        [GlobalApi]
        public async Task<InputTypeFullModel> GetInputTypeById([FromRoute] int inputTypeId)
        {
            return await _inputConfigServiceBase.GetInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("getByCode")]
        //[GlobalApi]
        public async Task<InputTypeFullModel> GetInputTypeByCode([FromQuery] string inputTypeCode)
        {
            return await _inputConfigServiceBase.GetInputType(inputTypeCode).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}")]
        public async Task<bool> UpdateInputType([FromRoute] int inputTypeId, [FromBody] InputTypeModel inputType)
        {
            return await _inputConfigServiceBase.UpdateInputType(inputTypeId, inputType).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}")]
        public async Task<bool> DeleteInputType([FromRoute] int inputTypeId)
        {
            return await _inputConfigServiceBase.DeleteInputType(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas")]
        public async Task<PageData<InputAreaModel>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigServiceBase.GetInputAreas(inputTypeId, keyword, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<InputAreaModel> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigServiceBase.GetInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/basicInfo")]
        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo([FromRoute] int inputTypeId)
        {
            return await _inputConfigServiceBase.GetInputTypeBasicInfo(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<InputTypeViewModel> GetInputTypeBasicInfo([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            return await _inputConfigServiceBase.GetInputTypeViewInfo(inputTypeId, inputTypeViewId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/views")]
        public async Task<int> InputTypeViewCreate([FromRoute] int inputTypeId, [FromBody] InputTypeViewModel model)
        {
            return await _inputConfigServiceBase.InputTypeViewCreate(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId, [FromBody] InputTypeViewModel model)
        {
            if (inputTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputConfigServiceBase.InputTypeViewUpdate(inputTypeViewId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/views/{inputTypeViewId}")]
        public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
        {
            if (inputTypeId <= 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inputConfigServiceBase.InputTypeViewDelete(inputTypeViewId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{inputTypeId}/inputareas")]
        public async Task<int> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigServiceBase.AddInputArea(inputTypeId, inputArea).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<bool> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
        {
            return await _inputConfigServiceBase.UpdateInputArea(inputTypeId, inputAreaId, inputArea).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/inputareas/{inputAreaId}")]
        public async Task<bool> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
        {
            return await _inputConfigServiceBase.DeleteInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields")]
        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inputConfigServiceBase.GetInputAreaFields(inputTypeId, inputAreaId, keyword, page, size).ConfigureAwait(true);
        }

        //[HttpGet]
        //[Route("{inputTypeId}/inputareas/{inputAreaId}/inputareafields/{inputAreaField}")]
        //public async Task<InputAreaFieldOutputFullModel> GetInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaField)
        //{
        //    return await _inputConfigServiceBase.GetInputAreaField(inputTypeId, inputAreaId, inputAreaField).ConfigureAwait(true); ;
        //}

        [HttpPost]
        [Route("{inputTypeId}/multifields")]
        public async Task<bool> UpdateMultiField([FromRoute] int inputTypeId, [FromBody] List<InputAreaFieldInputModel> fields)
        {
            return await _inputConfigServiceBase.UpdateMultiField(inputTypeId, fields).ConfigureAwait(true);
        }
    }
}
