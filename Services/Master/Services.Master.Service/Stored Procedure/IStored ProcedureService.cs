using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Services.Master.Model.StoredProcedure;

namespace VErp.Services.Master.Service.StoredProcedure
{
    public interface IStoredProcedureService
    {
        Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList(EnumModuleType moduleType);
        Task<bool> Create(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel);
        Task<bool> Update(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel);
        Task<bool> Drop(EnumModuleType moduleType, int type, StoredProcedureModel storedProcedureModel);

    }
}
