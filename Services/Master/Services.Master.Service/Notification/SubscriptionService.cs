using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;

namespace VErp.Services.Master.Service.Notification
{
    public interface ISubscriptionService
    {
        Task<long> AddSubscription(SubscriptionModel model);
        Task<IList<SubscriptionModel>> GetListByUserId(int userId);
        Task<bool> UnSubscription(long subscriptionId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly ObjectActivityLogFacade _activityLog;
        private readonly IMapper _mapper;

        public SubscriptionService(ActivityLogDBContext activityLogContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _activityLogContext = activityLogContext;
            _mapper = mapper;
            _activityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Notification);
        }

        public async Task<IList<SubscriptionModel>> GetListByUserId(int userId)
        {
            var query = _activityLogContext.Subscription.Where(x => x.UserId == userId).AsNoTracking();
            return await query.ProjectTo<SubscriptionModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<long> AddSubscription(SubscriptionModel model)
        {
            var exists = await _activityLogContext.Subscription.FirstOrDefaultAsync(x => x.UserId == model.UserId && x.BillTypeId == model.BillTypeId && x.ObjectTypeId == model.ObjectTypeId && x.ObjectId == model.ObjectId);
            if (exists != null)
                return exists.SubscriptionId;

            var entity = _mapper.Map<Subscription>(model);
            _activityLogContext.Subscription.Add(entity);
            await _activityLogContext.SaveChangesAsync();

            return entity.SubscriptionId;
        }

        public async Task<bool> UnSubscription(long subscriptionId)
        {
            var exists = await _activityLogContext.Subscription.FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
            if (exists == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            exists.IsDeleted = true;
            await _activityLogContext.SaveChangesAsync();

            return true;
        }
    }
}