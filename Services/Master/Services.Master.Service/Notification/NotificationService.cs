using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;
using NotificationEntity = ActivityLogDB.Notification;

namespace VErp.Services.Master.Service.Notification
{
    public interface INotificationService
    {
        Task<bool> AddNotification(NotificationAdditionalModel model);
        Task<IList<NotificationModel>> GetListByUserId();
        Task<long> GetCountNotification();
        Task<bool> MarkerReadNotification(long[] lsNotificationId, bool mark);
    }

    public class NotificationService : INotificationService
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _activityLog;
        private readonly IMapper _mapper;

        public NotificationService(ActivityLogDBContext activityLogContext, IMapper mapper, IActivityLogService activityLogService, ICurrentContextService currentContextService)
        {
            _activityLogContext = activityLogContext;
            _mapper = mapper;
            _activityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Notification);
            _currentContextService = currentContextService;
        }

        public async Task<IList<NotificationModel>> GetListByUserId()
        {
            // var query = from nf in _activityLogContext.Notification
            //             join l in _activityLogContext.UserActivityLog on nf.UserActivityLogId equals l.UserActivityLogId into lg
            //             from l in lg.DefaultIfEmpty()
            //             join s in _activityLogContext.Subscription on new { nf.UserId, l.ObjectId, l.ObjectTypeId, l.BillTypeId } equals new { s.UserId, s.ObjectId, s.ObjectTypeId, s.BillTypeId } into sg
            //             from s in sg.DefaultIfEmpty()
            //             where nf.UserId == _currentContextService.UserId
            //             select new NotificationModel
            //             {
            //                 CreatedDatetimeUtc = nf.CreatedDatetimeUtc.GetUnix(),
            //                 IsRead = nf.IsRead,
            //                 NotificationId = nf.NotificationId,
            //                 ReadDateTimeUtc = nf.ReadDateTimeUtc.GetUnix(),
            //                 SubscriptionId = s.SubscriptionId,
            //                 UserActivityLogId = nf.UserActivityLogId,
            //                 UserId = s.UserId
            //             };
            var query = _activityLogContext.Notification.Where(x => x.UserId == _currentContextService.UserId);

            return await query.AsNoTracking().OrderBy(x => x.IsRead).ThenByDescending(x => x.CreatedDatetimeUtc).ProjectTo<NotificationModel>(_mapper.ConfigurationProvider).Take(100).ToListAsync();
        }

        public async Task<long> GetCountNotification()
        {
            return (await GetListByUserId()).Where(x => !x.IsRead).Count();
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
                if (mark)
                    item.ReadDateTimeUtc = System.DateTime.UtcNow;
            }

            await _activityLogContext.SaveChangesAsync();
            return true;
        }
    }
}