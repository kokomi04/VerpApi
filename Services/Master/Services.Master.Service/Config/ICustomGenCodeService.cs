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
    public interface ICustomGenCodeService
    {
        Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword, int page, int size);
        
        Task<CustomGenCodeOutputModel> GetInfo(int objectGenCodeId);
        
        Task<bool> Update(int customGenCodeId, int currentUserId, CustomGenCodeInputModel model);
        
        Task<bool> Delete(int currentUserId,int customGenCodeId);
        
        Task<int> Create(int currentUserId, CustomGenCodeInputModel model);

        Task<CustomCodeModel> GenerateCode(int customGenCodeId, int lastValue, string code = "");

        PageData<ObjectType> GetAllObjectType();

        Task<CustomGenCodeOutputModel> GetCurrentConfig(int objectTypeId, int objectId);

        Task<bool> MapObjectCustomGenCode(int currentId, ObjectCustomGenCodeMapping req);

        Task<bool> DeleteMapObjectCustomGenCode(int currentId, ObjectCustomGenCodeMapping req);

        Task<bool> UpdateMultiConfig(int objectTypeId, Dictionary<int, int> data);

        Task<bool> ConfirmCode(int objectTypeId, int objectId);
    }
}
