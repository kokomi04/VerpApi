using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputConfigService
    {
        // Input type
        Task<InputTypeFullModel> GetInputType(int inputTypeId);

        Task<IList<InputTypeFullModel>> GetAllInputTypes();

        Task<InputTypeFullModel> GetInputType(string inputTypeCode);

        Task<PageData<InputTypeModel>> GetInputTypes(string keyword, int page, int size);
        Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList();

        Task<InputTypeGlobalSettingModel> GetInputGlobalSetting();
        Task<bool> UpdateInputGlobalSetting(InputTypeGlobalSettingModel data);

        Task<int> AddInputType(InputTypeModel data);
        Task<bool> UpdateInputType(int inputTypeId, InputTypeModel data);
        Task<bool> DeleteInputType(int inputTypeId);
        Task<int> CloneInputType(int inputTypeId);


        Task<int> InputTypeViewCreate(int inputTypeId, InputTypeViewModel model);
        Task<bool> InputTypeViewUpdate(int inputTypeViewId, InputTypeViewModel model);
        Task<bool> InputTypeViewDelete(int inputTypeViewId);
        Task<IList<InputTypeViewModelList>> InputTypeViewList(int inputTypeId);
        Task<InputTypeBasicOutput> GetInputTypeBasicInfo(int inputTypeId);
        Task<InputTypeViewModel> GetInputTypeViewInfo(int inputTypeId, int inputTypeViewId);

        Task<int> InputTypeGroupCreate(InputTypeGroupModel model);
        Task<bool> InputTypeGroupUpdate(int inputTypeGroupId, InputTypeGroupModel model);
        Task<bool> InputTypeGroupDelete(int inputTypeGroupId);
        Task<IList<InputTypeGroupList>> InputTypeGroupList();

        // Area
        Task<InputAreaModel> GetInputArea(int inputTypeId, int inputAreaId);
        Task<PageData<InputAreaModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size);
        Task<int> AddInputArea(int inputTypeId, InputAreaInputModel data);
        Task<bool> UpdateInputArea(int inputTypeId, int inputAreaId, InputAreaInputModel data);
        Task<bool> DeleteInputArea(int inputTypeId, int inputAreaId);

        // Field
        Task<PageData<InputFieldOutputModel>> GetInputFields(string keyword, int page, int size);
        Task<InputFieldInputModel> AddInputField(InputFieldInputModel data);
        Task<InputFieldInputModel> UpdateInputField(int inputFieldId, InputFieldInputModel data);
        Task<bool> DeleteInputField(int inputFieldId);
        Task<InputAreaFieldOutputFullModel> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId);
        Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size);
        Task<bool> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields);

    }
}
