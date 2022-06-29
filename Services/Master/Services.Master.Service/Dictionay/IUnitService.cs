﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;

namespace VErp.Services.Master.Service.Dictionay
{
    public interface IUnitService
    {
        Task<int> AddUnit(UnitInput data);
        Task<PageData<UnitOutput>> GetList(string keyword, EnumUnitStatus? unitStatusId, int page, int size, Clause filters = null);
        Task<IList<UnitOutput>> GetListByIds(IList<int> unitIds);
        Task<UnitOutput> GetUnitInfo(int unitId);
        Task<bool> UpdateUnit(int unitId, UnitInput data);
        Task<bool> DeleteUnit(int unitId);
    }
}
