using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.System
{
    public interface IProgramingFunctionHelperService
    {
        Task<IList<ProgramingFunctionBaseModel>> Sqls();
        Task<IList<ProgramingFunctionBaseModel>> UserSqls();
        Task<IList<ProgramingFunctionBaseModel>> GetAllSqls();
    }
    public class ProgramingFunctionHelperService : IProgramingFunctionHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProgramingFunctionHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<IList<ProgramingFunctionBaseModel>> Sqls()
        {
            return await _httpCrossService.Get<List<ProgramingFunctionBaseModel>>($"api/internal/InternalProgramingFunction/Sqls", new { });
        }
        public async Task<IList<ProgramingFunctionBaseModel>> UserSqls()
        {
            return await _httpCrossService.Get<List<ProgramingFunctionBaseModel>>($"api/internal/InternalUserProgramingFunction", new { });
        }
        public async Task<IList<ProgramingFunctionBaseModel>> GetAllSqls()
        {
            var sqlFunctions = await _httpCrossService.Get<List<ProgramingFunctionBaseModel>>($"api/internal/InternalProgramingFunction/Sqls", new { });
            var userSqlFunctions = await _httpCrossService.Get<List<ProgramingFunctionBaseModel>>($"api/internal/InternalUserProgramingFunction", new { });
            foreach (var function in userSqlFunctions)
            {
                var sqlFunction = sqlFunctions.FirstOrDefault(x => x.ProgramingFunctionName == function.ProgramingFunctionName);
                if (sqlFunction != null)
                    sqlFunctions.Remove(sqlFunction);
                sqlFunctions.Add(function);
            }
            return sqlFunctions;
        }
    }
}
