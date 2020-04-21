using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputAreaFieldService
    {
        Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaField);
        Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, InputAreaFieldInputModel data);
        Task<Enum> UpdateInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, int inputAreaFieldId, InputAreaFieldInputModel data);
        Task<Enum> DeleteInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, int inputAreaFieldId);
    }
}
