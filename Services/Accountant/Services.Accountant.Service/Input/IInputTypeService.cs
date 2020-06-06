using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputTypeService
    {
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
    }
}
