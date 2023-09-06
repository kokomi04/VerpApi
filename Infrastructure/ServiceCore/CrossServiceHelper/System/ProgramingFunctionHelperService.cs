using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.System
{
    public interface IProgramingFunctionHelperService
    {
        Task<IList<ProgramingFunctionBaseModel>> Sqls();
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
    }
}
