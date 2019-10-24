using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class ActivityService : IActivityService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public ActivityService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , ICurrentContextService currentContextService
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
            _asyncRunnerService = asyncRunnerService;
        }


        public void CreateActivityAsync(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject)
        {
            _asyncRunnerService.RunAsync<IActivityService>(a => a.CreateActivity(objectTypeId, objectId, message, oldJsonObject, newObject));
        }

        public async Task<Enum> CreateActivity(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject)
        {
            var userId = _currentContextService.UserId;
            var actionId = (int)_currentContextService.Action;

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var activity = new UserActivityLog()
                {
                    UserId = userId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    ActionId = actionId,
                    ObjectTypeId = (int)objectTypeId,
                    ObjectId = objectId,
                    Message = message
                };

                await _masterContext.UserActivityLog.AddAsync(activity);
                await _masterContext.SaveChangesAsync();

                var changeLog = Utils.GetJsonDiff(oldJsonObject, newObject);

                var change = new UserActivityLogChange()
                {
                    UserActivityLogId = activity.UserActivityLogId,
                    ObjectChange = changeLog
                };

                await _masterContext.UserActivityLogChange.AddAsync(change);

                await _masterContext.SaveChangesAsync();

                trans.Commit();

                return GeneralCode.Success;
            }
        }
    }
}
