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


namespace VErp.Services.Master.Service.Activity.Implement
{
    public class ActivityService : IActivityService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int _userId = 0;
        private EnumAction _action = EnumAction.View;
        private bool _isFirstCall = true;



        public ActivityService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        protected int UserId
        {
            get
            {
                if (!_isFirstCall)
                    return _userId;

                _isFirstCall = false;
                foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                {
                    if (claim.Type != "userId")
                        continue;

                    int.TryParse(claim.Value, out _userId);
                    break;
                }

                var method = (EnumMethod)Enum.Parse(typeof(EnumMethod), _httpContextAccessor.HttpContext.Request.Method, true);

                if (!_httpContextAccessor.HttpContext.Items.ContainsKey("action"))
                {

                    _action = (EnumAction)_httpContextAccessor.HttpContext.Items["action"];
                }
                else
                {
                    _action = method.GetDefaultAction();
                }

                return _userId;
            }
        }

        public async Task<Enum> CreateActivity(EnumObjectType objectTypeId, long objectId, string message, string oldJsonObject, object newObject)
        {
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var activity = new UserActivityLog()
                {
                    UserId = UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    ActionId = (int)_action,
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
                trans.Commit();
                return GeneralCode.Success;
            }
        }
    }
}
