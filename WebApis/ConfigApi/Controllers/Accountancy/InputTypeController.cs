﻿namespace ConfigApi.Controllers.Accountancy
{
    //[Route("api/accountancy/config/inputType")]

    //public class InputTypeConfigController : VErpBaseController
    //{
    //    private readonly IInputConfigService _inputConfigService;
    //    public InputTypeConfigController(IInputConfigService inputConfigService)
    //    {
    //        _inputConfigService = inputConfigService;
    //    }


    //    [HttpGet]
    //    [Route("groups")]
    //    public async Task<IList<InputTypeGroupList>> GetList()
    //    {
    //        return await _inputConfigService.InputTypeGroupList().ConfigureAwait(true);
    //    }

    //    [HttpPost]
    //    [Route("groups")]
    //    public async Task<int> InputTypeGroupCreate([FromBody] InputTypeGroupModel model)
    //    {
    //        return await _inputConfigService.InputTypeGroupCreate(model).ConfigureAwait(true);
    //    }

    //    [HttpPut]
    //    [Route("groups/{voucherTypeGroupId}")]
    //    public async Task<bool> GetInputType([FromRoute] int inputTypeGroupId, [FromBody] InputTypeGroupModel model)
    //    {
    //        return await _inputConfigService.InputTypeGroupUpdate(inputTypeGroupId, model).ConfigureAwait(true);
    //    }

    //    [HttpDelete]
    //    [Route("groups/{voucherTypeGroupId}")]
    //    public async Task<bool> DeleteInputTypeGroup([FromRoute] int inputTypeGroupId)
    //    {
    //        return await _inputConfigService.InputTypeGroupDelete(inputTypeGroupId).ConfigureAwait(true);
    //    }


    //    [HttpGet]
    //    [Route("")]
    //    public async Task<PageData<InputTypeModel>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
    //    {
    //        return await _inputConfigService.GetInputTypes(keyword, page, size).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("fields")]
    //    public async Task<PageData<InputFieldOutputModel>> GetAllFields([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
    //    {
    //        return await _inputConfigService.GetInputFields(keyword, page, size).ConfigureAwait(true);
    //    }

    //    [HttpPost]
    //    [Route("fields")]
    //    public async Task<int> AddInputField([FromBody] InputFieldInputModel inputAreaField)
    //    {
    //        return await _inputConfigService.AddInputField(inputAreaField).ConfigureAwait(true);
    //    }

    //    [HttpPut]
    //    [Route("fields/{voucherFieldId}")]
    //    public async Task<bool> UpdateInputField([FromRoute] int inputFieldId, [FromBody] InputFieldInputModel inputField)
    //    {
    //        return await _inputConfigService.UpdateInputField(inputFieldId, inputField).ConfigureAwait(true);
    //    }

    //    [HttpDelete]
    //    [Route("fields/{voucherFieldId}")]
    //    public async Task<bool> DeleteInputField([FromRoute] int inputFieldId)
    //    {
    //        return await _inputConfigService.DeleteInputField(inputFieldId).ConfigureAwait(true);
    //    }

    //    [HttpPost]
    //    [Route("")]
    //    public async Task<int> AddInputType([FromBody] InputTypeModel category)
    //    {
    //        return await _inputConfigService.AddInputType(category).ConfigureAwait(true);
    //    }

    //    [HttpPost]
    //    [Route("{voucherTypeId}/clone")]
    //    public async Task<int> CloneInputType([FromRoute] int inputTypeId)
    //    {
    //        return await _inputConfigService.CloneInputType(inputTypeId).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}")]
    //    public async Task<InputTypeFullModel> GetInputType([FromRoute] int inputTypeId)
    //    {
    //        return await _inputConfigService.GetInputType(inputTypeId).ConfigureAwait(true);
    //    }

    //    [HttpPut]
    //    [Route("{voucherTypeId}")]
    //    public async Task<bool> UpdateInputType([FromRoute] int inputTypeId, [FromBody] InputTypeModel inputType)
    //    {
    //        return await _inputConfigService.UpdateInputType(inputTypeId, inputType).ConfigureAwait(true);
    //    }

    //    [HttpDelete]
    //    [Route("{voucherTypeId}")]
    //    public async Task<bool> DeleteInputType([FromRoute] int inputTypeId)
    //    {
    //        return await _inputConfigService.DeleteInputType(inputTypeId).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/inputareas")]
    //    public async Task<PageData<InputAreaModel>> GetInputAreas([FromRoute] int inputTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
    //    {
    //        return await _inputConfigService.GetInputAreas(inputTypeId, keyword, page, size).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/inputareas/{voucherAreaId}")]
    //    public async Task<InputAreaModel> GetInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
    //    {
    //        return await _inputConfigService.GetInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/basicInfo")]
    //    public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo([FromRoute] int inputTypeId)
    //    {
    //        return await _inputConfigService.GetInputTypeBasicInfo(inputTypeId).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
    //    public async Task<InputTypeViewModel> GetInputTypeBasicInfo([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
    //    {
    //        return await _inputConfigService.GetInputTypeViewInfo(inputTypeId, inputTypeViewId).ConfigureAwait(true);
    //    }

    //    [HttpPost]
    //    [Route("{voucherTypeId}/views")]
    //    public async Task<int> InputTypeViewCreate([FromRoute] int inputTypeId, [FromBody] InputTypeViewModel model)
    //    {
    //        return await _inputConfigService.InputTypeViewCreate(inputTypeId, model).ConfigureAwait(true);
    //    }

    //    [HttpPut]
    //    [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
    //    public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId, [FromBody] InputTypeViewModel model)
    //    {
    //        return await _inputConfigService.InputTypeViewUpdate(inputTypeViewId, model).ConfigureAwait(true);
    //    }

    //    [HttpDelete]
    //    [Route("{voucherTypeId}/views/{voucherTypeViewId}")]
    //    public async Task<bool> InputTypeViewUpdate([FromRoute] int inputTypeId, [FromRoute] int inputTypeViewId)
    //    {
    //        return await _inputConfigService.InputTypeViewDelete(inputTypeViewId).ConfigureAwait(true);
    //    }


    //    [HttpPost]
    //    [Route("{voucherTypeId}/inputareas")]
    //    public async Task<int> AddInputArea([FromRoute] int inputTypeId, [FromBody] InputAreaInputModel inputArea)
    //    {
    //        return await _inputConfigService.AddInputArea(inputTypeId, inputArea).ConfigureAwait(true);
    //    }

    //    [HttpPut]
    //    [Route("{voucherTypeId}/inputareas/{voucherAreaId}")]
    //    public async Task<bool> UpdateInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromBody] InputAreaInputModel inputArea)
    //    {
    //        return await _inputConfigService.UpdateInputArea(inputTypeId, inputAreaId, inputArea).ConfigureAwait(true);
    //    }

    //    [HttpDelete]
    //    [Route("{voucherTypeId}/inputareas/{voucherAreaId}")]
    //    public async Task<bool> DeleteInputArea([FromRoute] int inputTypeId, [FromRoute] int inputAreaId)
    //    {
    //        return await _inputConfigService.DeleteInputArea(inputTypeId, inputAreaId).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/inputareas/{voucherAreaId}/inputareafields")]
    //    public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
    //    {
    //        return await _inputConfigService.GetInputAreaFields(inputTypeId, inputAreaId, keyword, page, size).ConfigureAwait(true);
    //    }

    //    [HttpGet]
    //    [Route("{voucherTypeId}/inputareas/{voucherAreaId}/inputareafields/{voucherAreaField}")]
    //    public async Task<InputAreaFieldOutputFullModel> GetInputAreaField([FromRoute] int inputTypeId, [FromRoute] int inputAreaId, [FromRoute] int inputAreaField)
    //    {
    //        return await _inputConfigService.GetInputAreaField(inputTypeId, inputAreaId, inputAreaField).ConfigureAwait(true); ;
    //    }

    //    [HttpPost]
    //    [Route("{voucherTypeId}/multifields")]
    //    public async Task<bool> UpdateMultiField([FromRoute] int inputTypeId, [FromBody] List<InputAreaFieldInputModel> fields)
    //    {
    //        return await _inputConfigService.UpdateMultiField(inputTypeId, fields).ConfigureAwait(true);
    //    }
    //}
}
