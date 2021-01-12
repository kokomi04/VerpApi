using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IObjectPrintConfigService
    {
        Task<PageData<ObjectPrintConfigSearch>> GetObjectPrintConfigSearch(string keyword, int page, int size);
        Task<bool> MapObjectPrintConfig(ObjectPrintConfig mapping);
        Task<ObjectPrintConfig> GetObjectPrintConfigMapping(EnumObjectType objectTypeId, int objectId);
    }
}
