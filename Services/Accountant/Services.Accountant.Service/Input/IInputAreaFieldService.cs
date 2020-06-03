using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputAreaFieldService
    {
        Task<PageData<InputFieldModel>> GetAll(string keyword, int page, int size);
        Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId);
        Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputAreaField(int inputTypeId, int inputAreaId, InputAreaFieldInputModel data);
        Task<Enum> UpdateInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId, InputAreaFieldInputModel data);
        Task<Enum> DeleteInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId);
        Task<ServiceResult<int>> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields);
    }
}
