using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;

namespace VErp.Services.Master.Service.Dictionay
{
    public interface IUnitService
    {
        Task<ServiceResult<int>> AddUnit(UnitInput data);
        Task<PageData<UnitOutput>> GetList(string keyword, EnumUnitStatus? unitStatusId, int page, int size, Dictionary<string, List<string>> filters = null);
        Task<IList<UnitOutput>> GetListByIds(IList<int> unitIds);
        Task<ServiceResult<UnitOutput>> GetUnitInfo(int unitId);
        Task<Enum> UpdateUnit(int unitId, UnitInput data);
        Task<Enum> DeleteUnit(int unitId);
    }
}
