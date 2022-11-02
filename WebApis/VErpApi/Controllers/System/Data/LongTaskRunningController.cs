using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.Data;
using VErp.Services.Master.Service.Data;

namespace VErpApi.Controllers.System.Data
{
    [Route("api/LongTaskRunning")]
    public class LongTaskRunningController : VErpBaseController
    {
      
        [HttpGet("Check")]
        [GlobalApi()]
        public ILongTaskResourceInfo Check()
        {
            LongTaskResourceLockFactory.WatchStatus();
            return LongTaskResourceLockFactory.GetCurrentProcess();
        }

    }
}
