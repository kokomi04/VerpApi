using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Accountant.Service.RawSQLQuery
{
    public interface IRawSQLQueryService
    {
        Task<ServiceResult<List<List<Dictionary<string, string>>>>> FromSQLRaw(string query);

        Task<ServiceResult<List<string>>> GetProcedures();

        Task<ServiceResult<string>> GetProcedure(string procedureName);

        Task<ServiceResult<List<List<Dictionary<string, string>>>>> ProcedureExec(string procedure,string[] parameter);
    }
}
