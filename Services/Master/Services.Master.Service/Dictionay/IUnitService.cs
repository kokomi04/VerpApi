using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;

namespace VErp.Services.Master.Service.Dictionay
{
    public interface IUnitService
    {
        Task<ServiceResult<int>> AddUnit(UnitInput data);
        Task<PageData<UnitOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<UnitOutput>> GetUnitInfo(int unitId);
        Task<Enum> UpdateUnit(int unitId, UnitInput data);
        Task<Enum> DeleteUnit(int unitId);
    }
}
