using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Notification;

namespace VErp.Services.Master.Service.Notification
{
    public interface ISubscriptionService
    {
        Task<long> AddSubscription(SubscriptionModel model);
        Task<bool> AddSubscriptionToThePermissionPerson(SubscriptionToThePermissionPersonSimpleModel req);
        Task<bool> CheckSubscription(CheckSubscriptionSimpleModel model);
        Task<IList<SubscriptionModel>> GetListByUserId();
        Task<bool> MarkerSubscription(CheckSubscriptionSimpleModel model, bool marker);
        Task<bool> UnSubscription(long subscriptionId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ActivityLogDBContext _activityLogContext;
        private readonly ObjectActivityLogFacade _activityLog;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly IUserHelperService _userHelperService;
        private readonly IRoleHelperService _roleHelperService;

        public SubscriptionService(ActivityLogDBContext activityLogContext, IMapper mapper, IActivityLogService activityLogService, ICurrentContextService currentContextService, IUserHelperService userHelperService, IRoleHelperService roleHelperService)
        {
            _activityLogContext = activityLogContext;
            _mapper = mapper;
            _activityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Notification);
            _currentContextService = currentContextService;
            _userHelperService = userHelperService;
            _roleHelperService = roleHelperService;
        }

        public async Task<bool> CheckSubscription(CheckSubscriptionSimpleModel model)
        {
            return await _activityLogContext.Subscription.AnyAsync(x => x.UserId == _currentContextService.UserId && x.BillTypeId == model.BillTypeId && x.ObjectTypeId == model.ObjectTypeId && x.ObjectId == model.ObjectId);
        }

        public async Task<bool> MarkerSubscription(CheckSubscriptionSimpleModel model, bool marker)
        {
            var exists = await _activityLogContext.Subscription.FirstOrDefaultAsync(x => x.UserId == _currentContextService.UserId && x.BillTypeId == model.BillTypeId && x.ObjectTypeId == model.ObjectTypeId && x.ObjectId == model.ObjectId);
            if (exists == null)
            {
                if (marker)
                {
                    await AddSubscription(new SubscriptionModel
                    {
                        BillTypeId = model.BillTypeId,
                        ObjectId = model.ObjectId,
                        ObjectTypeId = model.ObjectTypeId,
                        UserId = _currentContextService.UserId
                    });
                }
                return true;
            }

            if (!marker)
                exists.IsDeleted = true;

            _activityLogContext.SaveChanges();

            return true;
        }

        public async Task<IList<SubscriptionModel>> GetListByUserId()
        {
            var query = _activityLogContext.Subscription.Where(x => x.UserId == _currentContextService.UserId).AsNoTracking();
            return await query.ProjectTo<SubscriptionModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<long> AddSubscription(SubscriptionModel model)
        {
            if (model.UserId == 0)
                model.UserId = _currentContextService.UserId;

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

        public async Task<bool> AddSubscriptionToThePermissionPerson(SubscriptionToThePermissionPersonSimpleModel req)
        {
            var roles = await _roleHelperService.GetRolesPermissionByModuleAndPermission(req.ModuleId, req.PermissionId);
            var users = await _userHelperService.GetListByRoles(roles.Select(x => x.RoleId).ToList());

            var trans = await _activityLogContext.Database.BeginTransactionAsync();
            try
            {
                var userIds = users.Select(x => x.UserId);


                var arrExistsUserId = (await _activityLogContext.Subscription.Where(x => userIds.Contains(x.UserId) && x.BillTypeId == req.BillTypeId && x.ObjectTypeId == req.ObjectTypeId && x.ObjectId == req.ObjectId)
                    .ToListAsync())
                    .Select(x => x.UserId);

                var entities = users.Where(x => !arrExistsUserId.Contains(x.UserId)).Select(u => new Subscription
                {
                    BillTypeId = req.BillTypeId,
                    ObjectId = req.ObjectId,
                    ObjectTypeId = req.ObjectTypeId,
                    UserId = u.UserId
                }).ToList();

                _activityLogContext.Subscription.AddRange(entities);
                await _activityLogContext.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }
    }
}