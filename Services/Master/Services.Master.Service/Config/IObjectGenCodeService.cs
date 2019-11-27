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
    public interface IObjectGenCodeService
    {
        Task<PageData<ObjectGenCodeOutputModel>> GetList(EnumObjectType objectType,string keyword, int page, int size);
        
        Task<ServiceResult<ObjectGenCodeOutputModel>> GetInfo(int objectGenCodeId);
        
        Task<Enum> Update(int objectGenCodeId, int currentUserId, ObjectGenCodeInputModel model);
        
        Task<Enum> Delete(int currentUserId,int objectGenCodeId);
        
        Task<ServiceResult<int>> Create(EnumObjectType objectType, int currentUserId, ObjectGenCodeInputModel model);

        /// <summary>
        /// Sinh mã code theo loại đối tượng dựa vào cấu hình ObjectGenCode trong DB
        /// </summary>
        /// <param name="objectType">(Enum) object type</param>
        /// <returns>string code</returns>
        Task<ServiceResult<string>> GenerateCode(EnumObjectType objectType);

        Task<PageData<ObjectType>> GetAllObjectType();
    }
}
