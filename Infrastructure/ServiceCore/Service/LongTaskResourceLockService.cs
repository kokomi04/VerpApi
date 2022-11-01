using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface ILongTaskResourceLockService
    {
        Task<LongTaskResourceLock> Accquire(string processTaskName, int? totalSteps = null);
    }
    public class LongTaskResourceLockService : ILongTaskResourceLockService
    {
        private readonly ICurrentContextService _currentContext;

        public LongTaskResourceLockService(ICurrentContextService currentContext)
        {
            _currentContext = currentContext;
        }

        public async Task<LongTaskResourceLock> Accquire(string processTaskName, int? totalSteps = null)
        {
            return await LongTaskResourceLockFactory.Accquire(processTaskName, _currentContext.UserId, totalSteps);
        }
    }
}
