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
        Task<PageData<CustomGenCodeOutputModel>> GetList(string keyword, int page, int size, int? objectTypeId);
        
        Task<ServiceResult<CustomGenCodeOutputModel>> GetInfo(int objectGenCodeId);
        
        Task<Enum> Update(int customGenCodeId, int currentUserId, CustomGenCodeInputModel model);
        
        Task<Enum> Delete(int currentUserId,int customGenCodeId);
        
        Task<ServiceResult<int>> Create(int currentUserId, CustomGenCodeInputModel model);

        Task<ServiceResult<string>> GenerateCode(int objectTypeId, int objectId);

        Task<PageData<ObjectType>> GetAllObjectType();
    }
}
