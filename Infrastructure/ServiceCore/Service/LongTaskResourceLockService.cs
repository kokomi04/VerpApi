using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Infrastructure.ServiceCore.Service
{
    public interface ILongTaskResourceLockService
    {
        Task<LongTaskResourceLock> Accquire(string processTaskName, int? totalSteps = null);
    }
    public class LongTaskResourceLockService : ILongTaskResourceLockService
    {
        private readonly ICurrentContextService _currentContext;
        private readonly IUserHelperService _userHelperService;

        public LongTaskResourceLockService(ICurrentContextService currentContext, IUserHelperService userHelperService)
        {
            _currentContext = currentContext;
            _userHelperService = userHelperService;
        }

        public async Task<LongTaskResourceLock> Accquire(string processTaskName, int? totalSteps = null)
        {
            var userInfos = await _userHelperService.GetByIds(new[] { _currentContext.UserId });
            if (userInfos == null || userInfos.Count == 0) throw GeneralCode.Forbidden.BadRequest();

            var userInfo = userInfos[0];
            var creationInfo = new LongTaskCreationInfo(_currentContext.TraceIdentifier, userInfo.UserId, userInfo.UserName, userInfo.FullName, DateTime.UtcNow.GetUnix());
            return await LongTaskResourceLockFactory.Accquire(processTaskName, creationInfo, totalSteps);
        }
    }
}
