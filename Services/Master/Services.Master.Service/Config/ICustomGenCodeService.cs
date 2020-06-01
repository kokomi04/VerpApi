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
        
        Task<ServiceResult<CustomGenCodeOutputModel>> GetInfo(int objectGenCodeId);
        
        Task<Enum> Update(int customGenCodeId, int currentUserId, CustomGenCodeInputModel model);
        
        Task<Enum> Delete(int currentUserId,int customGenCodeId);
        
        Task<ServiceResult<int>> Create(int currentUserId, CustomGenCodeInputModel model);

        Task<ServiceResult<CustomCodeModel>> GenerateCode(int customGenCodeId, int lastValue);

        Task<PageData<ObjectType>> GetAllObjectType();

        Task<ServiceResult<CustomGenCodeOutputModel>> GetCurrentConfig(int objectTypeId, int objectId);

        Task<Enum> MapObjectCustomGenCode(int currentId, ObjectCustomGenCodeMapping req);

        Task<Enum> ConfirmCode(int objectTypeId, int objectId);
    }
}
