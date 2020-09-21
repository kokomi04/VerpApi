using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.StoredProcedure;

namespace VErp.Services.Accountancy.Service.StoredProcedure
{
    public interface IStoredProcedureService
    {
        Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList();
        Task<bool> Create(int type, StoredProcedureModel storedProcedureModel);
        Task<bool> Update(int type, StoredProcedureModel storedProcedureModel);
        Task<bool> Drop(int type, StoredProcedureModel storedProcedureModel);

    }
}
