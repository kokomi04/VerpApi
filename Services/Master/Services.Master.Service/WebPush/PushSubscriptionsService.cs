using ActivityLogDB;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Services.Master.Model.WebPush;
using PushSubscriptionEntity = ActivityLogDB.PushSubscription;

namespace VErp.Services.Master.Service.Webpush
{
    public interface IPushSubscriptionsService
    {
        Task<IList<PushSubscriptionModel>> GetAll();
        Task<IList<PushSubscriptionModel>> GetByArrUserId(int[] arrUserId);
        Task<bool> Subscribe(PushSubscriptionRequest subscription);
        Task<bool> UnSubscribe(string endpoint);

        Task<String> GetPublicKey();
    }


    public class PushSubscriptionsService : IPushSubscriptionsService
    {
        private readonly string KEY_AUTH = "auth";
        private readonly string KEY_P256DH = "p256dh";

        private readonly ActivityLogDBContext _activityLogContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;

        public PushSubscriptionsService(ActivityLogDBContext activityLogContext,
                                        IOptions<AppSetting> appSetting,
                                        ILogger<PushSubscriptionsService> logger,
                                        ICurrentContextService currentContextService,
                                        IMapper mapper)
        {
            _activityLogContext = activityLogContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
            _mapper = mapper;
        }

        public async Task<IList<PushSubscriptionModel>> GetAll()
        {
            return await _activityLogContext.PushSubscription.AsNoTracking().ProjectTo<PushSubscriptionModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<IList<PushSubscriptionModel>> GetByArrUserId(int[] arrUserId)
        {
            return await _activityLogContext.PushSubscription.Where(x => arrUserId.Contains(x.UserId)).AsNoTracking().ProjectTo<PushSubscriptionModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<string> GetPublicKey()
        {
            if (string.IsNullOrWhiteSpace(_appSetting.WebPush.PublicKey))
                throw new BadRequestException(GeneralCode.InternalError, "Hệ thống gửi thông báo chưa sẵn sàng");
            return await Task.FromResult(_appSetting.WebPush.PublicKey);
        }

        public async Task<bool> Subscribe(PushSubscriptionRequest subscription)
        {
            if (!_activityLogContext.PushSubscription.Any(x => x.Endpoint == subscription.Endpoint))
            {
                var entity = new PushSubscriptionEntity()
                {
                    Auth = subscription.Keys[KEY_AUTH],
                    Endpoint = subscription.Endpoint,
                    ExpirationTime = subscription.ExpirationTime,
                    P256dh = subscription.Keys[KEY_P256DH],
                    UserId = _currentContextService.UserId
                };

                _activityLogContext.PushSubscription.Add(entity);
                _activityLogContext.SaveChanges();
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> UnSubscribe(string endpoint)
        {
            var pushSubscription = _activityLogContext.PushSubscription.FirstOrDefault(x => x.Endpoint == endpoint);
            if (pushSubscription != null)
            {
                pushSubscription.IsDeleted = true;
                _activityLogContext.SaveChanges();
            }

            return await Task.FromResult(true);
        }

    }
}