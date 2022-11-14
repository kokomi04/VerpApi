using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Verp.Resources;
using VErp.Commons.Enums;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.SignalR;
using VErp.Services.Master.Model.Activity;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Model.WebPush;
using VErp.Services.Master.Service.Users;
using static VErp.Services.Master.Model.WebPush.AngularPushNotification;
using NotificationEntity = ActivityLogDB.Notification;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class UserLogActionService : IUserLogActionService
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly IUserService _userService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly IPrincipalBroadcasterService _principalBroadcaster;
        private readonly IHubContext<BroadcastSignalRHub, IBroadcastHubClient> _hubNotifyContext;
        private readonly PushServiceClient _pushClient;

        public UserLogActionService(ActivityLogDBContext activityLogContext
            , IUserService userService
            , IOptions<AppSetting> appSetting
            , ILogger<UserLogActionService> logger
            , IAsyncRunnerService asyncRunnerService
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IMapper mapper, IPrincipalBroadcasterService principalBroadcaster
            , IHubContext<BroadcastSignalRHub, IBroadcastHubClient> hubNotifyContext
            , PushServiceClient pushClient)
        {
            _activityLogContext = activityLogContext;
            _userService = userService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _asyncRunnerService = asyncRunnerService;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _principalBroadcaster = principalBroadcaster;
            _hubNotifyContext = hubNotifyContext;
            _pushClient = pushClient;

            if (_appSetting.WebPush != null && !string.IsNullOrWhiteSpace(_appSetting.WebPush.PublicKey) && !string.IsNullOrWhiteSpace(_appSetting.WebPush.PrivateKey))
                _pushClient.DefaultAuthentication = new VapidAuthentication(_appSetting.WebPush.PublicKey, _appSetting.WebPush.PrivateKey);
        }

        public void CreateActivityAsync(ActivityInput input)
        {
            _asyncRunnerService.RunAsync<IUserLogActionService>(a => a.CreateActivityTask(input));
        }

        public async Task<long> CreateActivityTask(ActivityInput input)
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

                var bodyNotification = new NotificationAdditionalModel
                {
                    BillTypeId = input.BillTypeId,
                    ObjectId = input.ObjectId,
                    ObjectTypeId = (int)input.ObjectTypeId,
                    UserActivityLogId = activity.UserActivityLogId
                };

                await AddNotification(bodyNotification, input.UserId, input);

                await _activityLogContext.SaveChangesAsync();

                trans.Commit();

                return activity.UserActivityLogId;
            }
        }


        public async Task<PageData<UserLoginLogModel>> GetUserLoginLogs(int pageIdex, int pageSize, string keyword, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters)
        {
            var query = _activityLogContext.UserLoginLog.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(l => l.UserName.Contains(keyword) || l.UserAgent.Contains(keyword));
            }

            if (fromDate > 0 && toDate > 0)
            {
                var fromDateTime = fromDate.UnixToDateTime().Value.AddMinutes(-_currentContextService.TimeZoneOffset.Value);
                var toDateTime = toDate.UnixToDateTime().Value.AddMinutes(-_currentContextService.TimeZoneOffset.Value);
                query = query.Where(l => l.CreatedDatetimeUtc.Date >= fromDateTime && l.CreatedDatetimeUtc.Date <= toDateTime);
            }

            if (string.IsNullOrEmpty(orderByFieldName))
            {
                orderByFieldName = "CreatedDatetimeUtc";
                asc = false;
            }

            if (filters != null)
            {
                query = query.InternalFilter(filters);
            }

            var total = query.Count();

            query = query.InternalOrderBy(orderByFieldName, asc);
            query = pageSize > 0 ? query.AsNoTracking().Skip((pageIdex - 1) * pageSize).Take(pageSize) : query.AsNoTracking();

            var result = await query.ProjectTo<UserLoginLogModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (result, total);
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


        public async Task<PageData<UserActivityLogOuputModel>> GetUserLogByObject(int? billTypeId, long objectId, EnumObjectType objectTypeId, int pageIdex = 1, int pageSize = 20)
        {
            return await GetListUserActivityLog(null, null, null, null, null, billTypeId, objectId, objectTypeId, null, null, false, pageIdex, pageSize);
        }

        public async Task<PageData<UserActivityLogOuputModel>> GetListUserActivityLog(long[] userActivityLogIds, string keyword, long? fromDate, long? toDate, int? userId, int? billTypeId, long? objectId, EnumObjectType? objectTypeId, int? actionTypeId, string sortBy, bool asc, int page = 1, int size = 20)
        {
            var query = _activityLogContext.UserActivityLog.AsNoTracking().AsQueryable();
            if (userActivityLogIds?.Length > 0)
            {
                query = query.Where(q => userActivityLogIds.Contains(q.UserActivityLogId));
            }
            if (fromDate.HasValue)
            {
                query = query.Where(q => q.CreatedDatetimeUtc >= fromDate.Value.UnixToDateTime());
            }
            if (toDate.HasValue)
            {
                query = query.Where(q => q.CreatedDatetimeUtc <= toDate.Value.UnixToDateTime());
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(q => q.Message.Contains(keyword));
            }
            if (userId.HasValue)
            {
                query = query.Where(q => q.UserId == userId.Value);
            }

            if (actionTypeId.HasValue)
            {
                query = query.Where(q => q.ActionId == actionTypeId.Value);
            }

            if (billTypeId.HasValue)
            {
                query = query.Where(q => q.BillTypeId == billTypeId.Value);
            }
            if (objectTypeId.HasValue)
            {
                query = query.Where(q => q.ObjectTypeId == (int)objectTypeId);
            }
            if (objectId.HasValue)
            {
                query = query.Where(q => q.ObjectId == objectId);
            }

            var properies = typeof(UserActivityLog).GetProperties();
            if (string.IsNullOrWhiteSpace(sortBy) || !properies.Any(p => p.Name == sortBy))
            {
                sortBy = nameof(UserActivityLog.UserActivityLogId);
            }

            var total = query.Count();

            query = query.SortByFieldName(sortBy, asc);


            var ualDataList = await (size > 0 ? query.AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync() : query.ToListAsync());

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
                    IpAddress = item.IpAddress,
                    UserActivityLogId = item.UserActivityLogId,
                    BillTypeId = item.BillTypeId,
                    ObjectId = item.ObjectId,
                    ObjectTypeId = (EnumObjectType)item.ObjectTypeId
                };
                result.Add(actLogOutput);
            }
            return (result, total);
        }

    
        private async Task AddNotification(NotificationAdditionalModel model, int userId, ActivityInput log)
        {
            var querySub = _activityLogContext.Subscription.Where(x => x.ObjectId == model.ObjectId && x.ObjectTypeId == model.ObjectTypeId && userId != x.UserId);
            if (model.BillTypeId.HasValue)
                querySub = querySub.Where(x => x.BillTypeId == model.BillTypeId);

            var lsSubscription = await querySub.ProjectTo<SubscriptionModel>(_mapper.ConfigurationProvider).ToListAsync();

            var lsNewNotification = lsSubscription.Select(x => new NotificationEntity
            {
                IsRead = false,
                UserId = x.UserId,
                UserActivityLogId = model.UserActivityLogId
            });

            _activityLogContext.Notification.AddRange(lsNewNotification);
            await _activityLogContext.SaveChangesAsync();

            await PushNotification(lsSubscription, log.Message, model);
        }

        private async Task PushNotification(IList<SubscriptionModel> lsSubscription, string message, NotificationAdditionalModel data)
        {
            try
            {
                var actionUrl = !string.IsNullOrWhiteSpace(_appSetting.WebPush.ActionUrl)
                    && (_appSetting.WebPush.ActionUrl.StartsWith("http://") || _appSetting.WebPush.ActionUrl.StartsWith("https://")) ? $"{_appSetting.WebPush.ActionUrl}redirect/{data.ObjectTypeId}/{data.ObjectId}/{data.BillTypeId}" : "";
                foreach (var sub in lsSubscription)
                {
                    var subUserId = sub.UserId;
                    if (_principalBroadcaster.IsUserConnected(subUserId.ToString()))
                        await _hubNotifyContext.Clients.Clients(_principalBroadcaster.GetAllConnectionId(new[] { subUserId.ToString() })).BroadcastMessage();
                    else if (_appSetting.WebPush != null && !string.IsNullOrWhiteSpace(_appSetting.WebPush.PublicKey) && !string.IsNullOrWhiteSpace(_appSetting.WebPush.PrivateKey))
                    {
                        var pushSubscriptions = await _activityLogContext.PushSubscription.AsNoTracking().Where(x => x.UserId == subUserId).ToListAsync();
                        foreach (var pushSubscription in pushSubscriptions)
                        {
                            PushMessage notification = new AngularPushNotification
                            {
                                Title = "VERP Thông Báo",
                                Body = message,
                                NotifyData = data,
                                Actions = !string.IsNullOrWhiteSpace(actionUrl) ? new NotificationAction[] { new NotificationAction(actionUrl, "Xem") } : new NotificationAction[] { },
                                Icon = "https://verp.vn/pic/Settings/log_63712_637654394979921899.png"
                            }.ToPushMessage();

                            var keys = new Dictionary<string, string>();
                            keys.Add("auth", pushSubscription.Auth);
                            keys.Add("p256dh", pushSubscription.P256dh);

                            // Fire-and-forget 
                            await _pushClient.RequestPushMessageDeliveryAsync(new Lib.Net.Http.WebPush.PushSubscription()
                            {
                                Endpoint = pushSubscription.Endpoint,
                                Keys = keys,
                            }, notification);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddNotification");
            }
        }
    }
}
