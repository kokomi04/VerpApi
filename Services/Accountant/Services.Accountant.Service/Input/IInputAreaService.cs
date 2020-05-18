using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input
{
    public interface IInputAreaService
    {
        Task<ServiceResult<InputAreaModel>> GetInputArea(int inputTypeId, int inputAreaId);
        Task<PageData<InputAreaModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size);
        Task<ServiceResult<int>> AddInputArea(int inputTypeId, InputAreaInputModel data);
        Task<Enum> UpdateInputArea(int inputTypeId, int inputAreaId, InputAreaInputModel data);
        Task<Enum> DeleteInputArea(int inputTypeId, int inputAreaId);
    }
}
