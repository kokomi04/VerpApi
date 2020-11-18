using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IGenCodeConfigService
    {
        Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword, int page, int size);
        
        Task<CustomGenCodeOutputModel> GetInfo(int customGenCodeId);
        
        Task<bool> Update(int customGenCodeId, CustomGenCodeInputModel model);
        
        Task<bool> Delete(int customGenCodeId);
        
        Task<int> Create(CustomGenCodeInputModel model);

        Task<CustomCodeModel> GenerateCode(int customGenCodeId, int lastValue, string code = "");

        Task<bool> ConfirmCode(int customGenCodeId);
    }
}
