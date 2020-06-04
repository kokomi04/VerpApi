using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputAreaFieldService
    {
        Task<PageData<InputFieldOutputModel>> GetInputFields(string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputField(InputFieldInputModel data);
        Task<Enum> UpdateInputField(int inputFieldId, InputFieldInputModel data);
        Task<Enum> DeleteInputField(int inputFieldId);

        Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId);
        Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size);
       
        Task<ServiceResult<int>> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields);
    }
}
