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
        
        Task<ObjectGenCodeOutputModel> GetInfo(int objectGenCodeId);
        
        Task<bool> Update(int objectGenCodeId, int currentUserId, ObjectGenCodeInputModel model);
        
        Task<bool> Delete(int currentUserId,int objectGenCodeId);
        
        Task<int> Create(EnumObjectType objectType, int currentUserId, ObjectGenCodeInputModel model);

        /// <summary>
        /// Sinh mã code theo loại đối tượng dựa vào cấu hình ObjectGenCode trong DB
        /// </summary>
        /// <param name="objectType">(Enum) object type</param>
        /// <returns>string code</returns>
        Task<string> GenerateCode(EnumObjectType objectType);

        PageData<ObjectType> GetAllObjectType();
    }
}
