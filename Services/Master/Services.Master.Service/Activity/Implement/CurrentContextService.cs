using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Services.Master.Service.Activity.Implement
{
    public class CurrentContextService : ICurrentContextService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int _userId = 0;
        private EnumAction? _action;

        public CurrentContextService(
            IOptions<AppSetting> appSetting
            , ILogger<ActivityService> logger
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                if (_userId > 0)
                    return _userId;

                foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                {
                    if (claim.Type != "userId")
                        continue;

                    int.TryParse(claim.Value, out _userId);
                    break;
                }
                return _userId;
            }
        }

        public EnumAction Action
        {
            get
            {
                if (_action.HasValue)
                {
                    return _action.Value;
                }

                var method = (EnumMethod)Enum.Parse(typeof(EnumMethod), _httpContextAccessor.HttpContext.Request.Method, true);

                if (_httpContextAccessor.HttpContext.Items.ContainsKey("action"))
                {

                    _action = (EnumAction)_httpContextAccessor.HttpContext.Items["action"];
                }
                else
                {
                    _action = method.GetDefaultAction();
                }

                return _action.Value;
            }
        }

    }
}
