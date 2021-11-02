using ActivityLogDB;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Activity;
using VErp.Services.Master.Service.Users;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class ActivityService : IActivityService
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly IUserService _userService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public ActivityService(ActivityLogDBContext activityLogContext
            , IUserService userService
            , IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , IAsyncRunnerService asyncRunnerService
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            )
        {
            _activityLogContext = activityLogContext;
            _userService = userService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _asyncRunnerService = asyncRunnerService;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public void CreateActivityAsync(ActivityInput input)
        {
            _asyncRunnerService.RunAsync<IActivityService>(a => a.CreateActivityTask(input));
        }

        public async Task<bool> CreateActivityTask(ActivityInput input)
        {
            using (var trans = await _activityLogContext.Database.BeginTransactionAsync())
            {
                var activity = new UserActivityLog()
                {
                    UserId = input.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    ActionId = (int)input.ActionId,
                    BillTypeId = input.BillTypeId,
                    ObjectTypeId = (int)input.ObjectTypeId,
                    MessageTypeId = (int)input.MessageTypeId,
                    ObjectId = input.ObjectId,
                    Message = input.Message,
                    MessageResourceName = input.MessageResourceName,
                    MessageResourceFormatData = input.MessageResourceFormatData,
                    SubsidiaryId = input.SubsidiaryId,
                    IpAddress = input.IpAddress,
                };

                await _activityLogContext.UserActivityLog.AddAsync(activity);
                await _activityLogContext.SaveChangesAsync();

                // var changeLog = Utils.GetJsonDiff(oldJsonObject, newObject);
                if (!string.IsNullOrWhiteSpace(input.Data))
                {
                    var change = new UserActivityLogChange()
                    {
                        UserActivityLogId = activity.UserActivityLogId,
                        ObjectChange = input.Data,//changeLog
                    };

                    await _activityLogContext.UserActivityLogChange.AddAsync(change);
                }
                await _activityLogContext.SaveChangesAsync();

                trans.Commit();

                return true;
            }
        }

        public async Task<bool> AddNote(int? billTypeId, long objectId, int objectTypeId, string message)
        {
            var activity = new UserActivityLog()
            {
                UserId = _currentContextService.UserId,
                BillTypeId = billTypeId,
                ObjectTypeId = objectTypeId,
                ObjectId = objectId,
                ActionId = (int)EnumActionType.View,
                MessageTypeId = (int)EnumMessageType.Comment,
                Message = message,
                MessageResourceName = string.Empty,
                MessageResourceFormatData = string.Empty,
                CreatedDatetimeUtc = DateTime.UtcNow,
                SubsidiaryId = _currentContextService.SubsidiaryId,
                IpAddress = _currentContextService.IpAddress
            };

            await _activityLogContext.UserActivityLog.AddAsync(activity);
            await _activityLogContext.SaveChangesAsync();

            return true;
        }
        

        public async Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(int? billTypeId, long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20)
        {
            var query = _activityLogContext.UserActivityLog.AsQueryable();
            if (billTypeId.HasValue)
            {
                query = query.Where(q => q.BillTypeId == billTypeId.Value);
            }

            query = query.Where(q => q.ObjectId == objectId && q.ObjectTypeId == (int)objectTypeId).OrderByDescending(q => q.UserActivityLogId);

            var total = query.Count();
            var ualDataList = pageSize > 0 ? query.AsNoTracking().Skip((pageIdex - 1) * pageSize).Take(pageSize).ToList() : query.AsNoTracking().ToList();

            var userIds = ualDataList.Select(q => q.UserId).ToList();

            var userInfos = (await _userService.GetBasicInfos(userIds))
                 .ToDictionary(u => u.UserId, u => u);

            var result = new List<UserActivityLogOuputModel>(ualDataList.Count);

            var resouces = new Dictionary<string, ResourceManager>();
            foreach (var item in ualDataList)
            {
                var message = item.Message;
                if (!string.IsNullOrWhiteSpace(item.MessageResourceName))
                {
                    string format = "";
                    try
                    {
                        var data = item.MessageResourceFormatData.JsonDeserialize<object[]>();
                        data = _activityLogService.ParseActivityLogData(data);
                        format = ResourcesAssembly.GetResouceString(item.MessageResourceName);
                        if (!string.IsNullOrWhiteSpace(format))
                        {
                            message = string.Format(format, data);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "ResourceFormat {0}", format);
                    }
                }

                userInfos.TryGetValue(item.UserId, out var userInfo);
                var actLogOutput = new UserActivityLogOuputModel
                {
                    UserId = item.UserId,
                    UserName = userInfo?.UserName,
                    FullName = userInfo?.FullName,
                    AvatarFileId = userInfo?.AvatarFileId,
                    ActionId = (EnumActionType?)item.ActionId,
                    Message = message,
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    MessageTypeId = (EnumMessageType)item.MessageTypeId,
                    //MessageResourceName = item.MessageResourceName,
                    //MessageResourceFormatData = item.MessageResourceFormatData,
                    SubsidiaryId = item.SubsidiaryId,
                    IpAddress = item.IpAddress
                };
                result.Add(actLogOutput);
            }
            return (result, total);
        }
    }
}
