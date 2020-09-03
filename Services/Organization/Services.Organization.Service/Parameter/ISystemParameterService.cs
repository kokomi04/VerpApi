using Services.Organization.Model.SystemParameter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;

namespace Services.Organization.Service.Parameter
{
    public interface ISystemParameterService
    {
        Task<PageData<SystemParameterModel>> GetList(string keyword, int page, int size);
        Task<SystemParameterModel> GetSystemParameterById(int keyId);
        Task<bool> UpdateSystemParameter(int keyId, SystemParameterModel systemParameterModel);
        Task<bool> DeleteSystemParameter(int keyId);
        Task<int> CreateSystemParameter(SystemParameterModel systemParameterModel);
    }
}
