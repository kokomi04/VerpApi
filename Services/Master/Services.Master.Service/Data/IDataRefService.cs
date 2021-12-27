using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Master.Model.Data;

namespace VErp.Services.Master.Service.Data
{
    public interface IDataRefService
    {
        Task<IList<DataRefModel>> GetDataRef(EnumObjectType objectTypeId, long? id, string code);
    }
}
