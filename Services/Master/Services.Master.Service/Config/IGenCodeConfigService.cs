using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IGenCodeConfigService
    {
        Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword, int page, int size);

        Task<CustomGenCodeOutputModel> GetInfo(int customGenCodeId, long? fId, string code, long? date);

        Task<PageData<CustomGenCodeBaseValueModel>> GetBaseValues(int customGenCodeId, long? fId, string code, long? date, int page, int size);

        Task<bool> Update(int customGenCodeId, CustomGenCodeInputModel model);

        Task<bool> SetLastValue(int customGenCodeId, CustomGenCodeBaseValueModel model);
        Task<bool> DeleteLastValue(int customGenCodeId, string baseValue);

        Task<bool> Delete(int customGenCodeId);

        Task<int> Create(CustomGenCodeInputModel model);

        Task<CustomCodeGeneratedModel> GenerateCode(int customGenCodeId, int lastValue, long? fId, string code, long? date);

        Task<bool> ConfirmCode(int customGenCodeId, string baseValue);
        
    }
}
