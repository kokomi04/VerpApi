using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class ActivityService : IActivityService
    {
        private readonly MasterDBContext _masterContext;
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public ActivityService(MasterDBContext masterContext
            , OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _organizationContext = organizationContext;
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _asyncRunnerService = asyncRunnerService;
        }

        public void CreateActivityAsync(ActivityInput input)
        {
            CreateActivityTask(input).ConfigureAwait(false);
        }

        public async Task<Enum> CreateActivityTask(ActivityInput input)
        {
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var activity = new UserActivityLog()
                {
                    UserId = input.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    ActionId = (int)input.ActionId,
                    ObjectTypeId = (int)input.ObjectTypeId,
                    MessageTypeId = (int)input.MessageTypeId,
                    ObjectId = input.ObjectId,
                    Message = input.Message
                };

                await _masterContext.UserActivityLog.AddAsync(activity);
                await _masterContext.SaveChangesAsync();

                // var changeLog = Utils.GetJsonDiff(oldJsonObject, newObject);
                if (!string.IsNullOrWhiteSpace(input.Data))
                {
                    var change = new UserActivityLogChange()
                    {
                        UserActivityLogId = activity.UserActivityLogId,
                        ObjectChange = input.Data,//changeLog
                    };

                    await _masterContext.UserActivityLogChange.AddAsync(change);
                }
                await _masterContext.SaveChangesAsync();

                trans.Commit();

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> CreateUserActivityLog(long objectId, int objectTypeId, int userId, int actionTypeId, EnumMessageType messageTypeId, string message)
        {
            var activity = new UserActivityLog()
            {
                UserId = userId,
                ObjectTypeId = objectTypeId,
                ObjectId = objectId,
                ActionId = actionTypeId,
                MessageTypeId = (int)messageTypeId,
                Message = message,
                CreatedDatetimeUtc = DateTime.UtcNow,
            };

            await _masterContext.UserActivityLog.AddAsync(activity);
            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20)
        {
            var query = _masterContext.UserActivityLog.Where(q => q.ObjectId == objectId && q.ObjectTypeId == (int)objectTypeId).OrderByDescending(q => q.UserActivityLogId);

            var total = query.Count();
            var ualDataList = query.AsNoTracking().Skip((pageIdex - 1) * pageSize).Take(pageSize).ToList();

            var userIdList = ualDataList.Select(q => q.UserId).ToList();
            var userDataList = (
                from u in _masterContext.User
                join e in _organizationContext.Employee on u.UserId equals e.UserId
                where userIdList.Contains(u.UserId)
                select new
                {
                    u.UserId,
                    u.UserName,
                    e.FullName,
                    e.AvatarFileId
                }).ToList()
                .ToDictionary(u => u.UserId, u => u);

            var result = new List<UserActivityLogOuputModel>(ualDataList.Count);
            foreach (var item in ualDataList)
            {
                userDataList.TryGetValue(item.UserId, out var userInfo);
                var actLogOutput = new UserActivityLogOuputModel
                {
                    UserId = item.UserId,
                    UserName = userInfo?.UserName,
                    FullName = userInfo?.FullName,
                    AvatarFileId = userInfo?.AvatarFileId,
                    ActionId = (EnumAction?)item.ActionId,
                    Message = item.Message,
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    MessageTypeId = (EnumMessageType)item.MessageTypeId,
                };
                result.Add(actLogOutput);
            }
            return (result, total);
        }
    }
}
