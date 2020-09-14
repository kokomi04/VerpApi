using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputConfigService
    {
        // Input type
        Task<ServiceResult<InputTypeFullModel>> GetInputType(int inputTypeId);
        Task<PageData<InputTypeModel>> GetInputTypes(string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputType(InputTypeModel data);
        Task<Enum> UpdateInputType(int inputTypeId, InputTypeModel data);
        Task<Enum> DeleteInputType(int inputTypeId);
        Task<ServiceResult<int>> CloneInputType(int inputTypeId);
        Task<int> InputTypeViewCreate(int inputTypeId, InputTypeViewModel model);
        Task<Enum> InputTypeViewUpdate(int inputTypeViewId, InputTypeViewModel model);
        Task<Enum> InputTypeViewDelete(int inputTypeViewId);
        Task<IList<InputTypeViewModelList>> InputTypeViewList(int inputTypeId);
        Task<InputTypeBasicOutput> GetInputTypeBasicInfo(int inputTypeId);
        Task<InputTypeViewModel> GetInputTypeViewInfo(int inputTypeId, int inputTypeViewId);
        Task<int> InputTypeGroupCreate(InputTypeGroupModel model);
        Task<bool> InputTypeGroupUpdate(int inputTypeGroupId, InputTypeGroupModel model);
        Task<bool> InputTypeGroupDelete(int inputTypeGroupId);
        Task<IList<InputTypeGroupList>> InputTypeGroupList();

        // Area
        Task<ServiceResult<InputAreaModel>> GetInputArea(int inputTypeId, int inputAreaId);
        Task<PageData<InputAreaModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputArea(int inputTypeId, InputAreaInputModel data);
        Task<Enum> UpdateInputArea(int inputTypeId, int inputAreaId, InputAreaInputModel data);
        Task<Enum> DeleteInputArea(int inputTypeId, int inputAreaId);

        // Field
        Task<PageData<InputFieldOutputModel>> GetInputFields(string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputField(InputFieldInputModel data);
        Task<Enum> UpdateInputField(int inputFieldId, InputFieldInputModel data);
        Task<Enum> DeleteInputField(int inputFieldId);
        Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId);
        Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size);
        Task<ServiceResult<int>> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields);

    }
}
