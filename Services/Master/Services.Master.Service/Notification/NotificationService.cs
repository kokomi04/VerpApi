using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;
using NotificationEntity = ActivityLogDB.Notification;

namespace VErp.Services.Master.Service.Notification
{
    public interface INotificationService
    {
        Task<bool> AddNotification(NotificationAdditionalModel model);
        Task<IList<NotificationModel>> GetListByUserId(int userId);
        Task<bool> MarkerReadNotification(long[] lsNotificationId, bool mark);
    }

    public class NotificationService : INotificationService
    {
        private readonly ActivityLogDBContext _activityLogContext;

        private readonly ObjectActivityLogFacade _activityLog;
        private readonly IMapper _mapper;

        public NotificationService(ActivityLogDBContext activityLogContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _activityLogContext = activityLogContext;
            _mapper = mapper;
            _activityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Notification);
        }

        public async Task<IList<NotificationModel>> GetListByUserId(int userId)
        {
            var query = _activityLogContext.Notification.Where(x => userId == x.UserId && x.IsRead == false);

            return await query.AsNoTracking().OrderBy(x => x.CreatedDatetimeUtc).ProjectTo<NotificationModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<bool> AddNotification(NotificationAdditionalModel model)
        {
            var querySub = _activityLogContext.Subscription.Where(x => x.ObjectId == model.ObjectId && x.ObjectTypeId == model.ObjectTypeId);
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

            return true;
        }

        public async Task<bool> MarkerReadNotification(long[] lsNotificationId, bool mark)
        {
            var lsNotification = await _activityLogContext.Notification.Where(x => lsNotificationId.Contains(x.NotificationId)).ToListAsync();
            foreach (var item in lsNotification)
            {
                item.IsRead = mark;
            }

            await _activityLogContext.SaveChangesAsync();
            return true;
        }
    }
}