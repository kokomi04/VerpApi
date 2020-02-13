using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Infrastructure.ServiceCore.Service
{


    public class CurrentContextFactory : ICurrentContextFactory
    {
        private ICurrentContextService _currentContext;
        private bool _used = false;

        public CurrentContextFactory(HttpCurrentContextService currentContext)
        {
            _currentContext = currentContext;
        }

        public void SetCurrentContext(ICurrentContextService currentContext)
        {
            _currentContext = currentContext;
        }

        public ICurrentContextService GetCurrentContext()
        {
            if (_used || _currentContext == null)
            {
                throw new InvalidOperationException();
            }

            _used = true;
            return _currentContext;
        }
    }



    public class HttpCurrentContextService : ICurrentContextService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterDBContext _masterDBContext;

        private int _userId = 0;
        private EnumAction? _action;
        private IList<int> _stockIds;

        public HttpCurrentContextService(
            IOptions<AppSetting> appSetting
            , ILogger<HttpCurrentContextService> logger
            , IHttpContextAccessor httpContextAccessor
            , MasterDBContext masterDBContext
            )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _masterDBContext = masterDBContext;
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

        public IList<int> StockIds
        {
            get
            {
                if (_stockIds != null)
                {
                    return _stockIds;
                }

                var userInfo = _masterDBContext.User.FirstOrDefault(u => u.UserId == UserId);

                _stockIds = _masterDBContext.RoleDataPermission
                    .Where(d => d.ObjectTypeId == (int)EnumObjectType.Stock && d.RoleId == userInfo.RoleId)
                    .Select(d => d.ObjectId)
                    .ToList()
                    .Select(d => (int)d)
                    .ToArray();

                return _stockIds;
            }
        }
    }

    public class ScopeCurrentContextService : ICurrentContextService
    {
        public ScopeCurrentContextService(int userId, EnumAction action, IList<int> stockIds)
        {
            UserId = userId;
            Action = action;
            StockIds = stockIds;
        }

        public int UserId { get; } = 0;

        public EnumAction Action { get; }

        public IList<int> StockIds { get; }
    }
}
