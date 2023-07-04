using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IObjectGenCodeService
    {
        Task<PageData<ObjectGenCodeMappingTypeModel>> GetObjectGenCodeMappingTypes(EnumModuleType? moduleTypeId, string keyword, int page, int size);

        Task<CustomGenCodeOutputModel> GetCurrentConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, long configObjectId, string configObjectTitle, long? fId, string code, long? date);

        public Task<bool> MapObjectGenCode(ObjectGenCodeMapping model);

        Task<bool> UpdateMultiConfig(EnumObjectType targetObjectTypeId, EnumObjectType configObjectTypeId, Dictionary<long, int> objectCustomGenCodes);

        public Task<bool> DeleteMapObjectGenCode(int objectCustomGenCodeMappingId);
    }
}
