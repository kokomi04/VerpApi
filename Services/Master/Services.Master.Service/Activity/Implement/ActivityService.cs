using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class ActivityService : IActivityService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public ActivityService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _asyncRunnerService = asyncRunnerService;
        }

        public void CreateActivityAsync(ActivityInput input)
        {
            _asyncRunnerService.RunAsync<IActivityService>(a => a.CreateActivityTask(input));
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
                    ObjectId = input.ObjectId,
                    Message = input.Message
                };

                await _masterContext.UserActivityLog.AddAsync(activity);
                await _masterContext.SaveChangesAsync();

                // var changeLog = Utils.GetJsonDiff(oldJsonObject, newObject);

                var change = new UserActivityLogChange()
                {
                    UserActivityLogId = activity.UserActivityLogId,
                    ObjectChange = input.Data,//changeLog
                };

                await _masterContext.UserActivityLogChange.AddAsync(change);

                await _masterContext.SaveChangesAsync();

                trans.Commit();

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> CreateUserActivityLog(long objectId,int objectTypeId,int userId,int actionTypeId,string message)
        {
            var activity = new UserActivityLog()
            {
                UserId = userId,
                ObjectTypeId = objectTypeId,
                ObjectId = objectId,
                ActionId = actionTypeId,
                Message = message,
                CreatedDatetimeUtc = DateTime.UtcNow,
            };

            await _masterContext.UserActivityLog.AddAsync(activity);
            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(long objectId, int objectTypeId, int pageIdex = 1, int pageSize = 20)
        {
            var query = _masterContext.UserActivityLog.Where(q => q.ObjectId == objectId && q.ObjectTypeId == objectTypeId).OrderByDescending(q=>q.UserActivityLogId);

            var total = query.Count();
            var ualDataList = query.AsNoTracking().Skip((pageIdex - 1) * pageSize).Take(pageSize).ToList();

            var userIdList = ualDataList.Select(q => q.UserId).ToList();
            var userDataList = _masterContext.User.Where(q => userIdList.Contains(q.UserId)).Select(q=> new { 
                q.UserId,
                q.UserName
            }).ToList();

            var result = new List<UserActivityLogOuputModel>(ualDataList.Count);
            foreach (var item in ualDataList)
            {
                var actLogOutput = new UserActivityLogOuputModel
                {
                    UserId = item.UserId,
                    UserName = userDataList.FirstOrDefault(q => q.UserId == item.UserId)?.UserName,
                    ObjectId = item.ObjectId,
                    ActionId = item.ActionId ?? 0,
                    ActionName = GetActionName(item.ActionId ?? 0),
                    Message = item.Message,
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix()
                };
                result.Add(actLogOutput);
            }
            return (result, total);
        }

        private string GetActionName(int actionTypeId)
        {
            var actionName = string.Empty;
            switch(actionTypeId)
            {
                case 1:
                    actionName = "Xem";
                        break;
                case 2:
                    actionName = "Thêm mới";
                    break;
                case 4:
                    actionName = "Sửa";
                    break;
                case 8:
                    actionName = "Xóa";
                    break;
                case 16:
                    actionName = "Duyệt";
                    break;
            }
            return actionName;
        }
    }
}
