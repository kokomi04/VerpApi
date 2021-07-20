using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using Microsoft.Extensions.Primitives;
using VErp.Commons.Constants;
using System.Security.Claims;
using System.Security.Principal;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.EF.OrganizationDB;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly Func<UnAuthorizeMasterDBContext> _masterDBContext;
        private readonly Func<UnAuthorizeOrganizationContext> _organizationDBContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private int _userId = 0;
        private string _userName = "";
        private int _subsidiaryId = 0;
        private EnumActionType? _action;
        private int? _timeZoneOffset = null;
        private IList<int> _stockIds;
        //private IList<int> _roleIds;
        private RoleInfo _roleInfo;
        private string _language;

        public HttpCurrentContextService(
            IOptions<AppSetting> appSetting
            , ILogger<HttpCurrentContextService> logger
            , IHttpContextAccessor httpContextAccessor
            , Func<UnAuthorizeMasterDBContext> masterDBContext
            , Func<UnAuthorizeOrganizationContext> organizationDBContext
           , IServiceScopeFactory serviceScopeFactory
            )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _masterDBContext = masterDBContext;
            _organizationDBContext = organizationDBContext;
            _serviceScopeFactory = serviceScopeFactory;
            CrossServiceLogin();
        }


        private void CrossServiceLogin()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null) return;

            var headers = httpContext.Request.Headers;
            headers.TryGetValue(Headers.CrossServiceKey, out var crossServiceKeys);
            if (crossServiceKeys.ToString() != _appSetting?.Configuration?.InternalCrossServiceKey)
            {
                return;
            }

            var userId = 0;
            var action = EnumActionType.View;

            if (headers.TryGetValue(Headers.UserId, out var strUserId))
            {
                userId = int.Parse(strUserId);
            }

            if (headers.TryGetValue(Headers.Action, out var strAction))
            {
                action = (EnumActionType)int.Parse(strAction);
            }

            if (headers.TryGetValue(Headers.SubsidiaryId, out var strSubsidiaryId))
            {
                _subsidiaryId = int.Parse(strSubsidiaryId);
            }
            if (headers.TryGetValue(Headers.Language, out var language))
            {
                _language = language;
            }

            if (userId > 0)
            {
                _userId = userId;
                var claims = new List<Claim>() {
                    new Claim(UserClaimConstants.UserId, userId + ""),
                    new Claim(UserClaimConstants.SubsidiaryId, _subsidiaryId + "")
                };

                var user = new GenericPrincipal(new ClaimsIdentity(claims), null);
                httpContext.User = user;

                if (!httpContext.Items.ContainsKey(HttpContextActionConstants.Action))
                {
                    httpContext.Items.Add(HttpContextActionConstants.Action, action);
                }

            }
        }

        public int UserId
        {
            get
            {
                if (_userId > 0)
                    return _userId;

                foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                {
                    if (claim.Type != UserClaimConstants.UserId)
                        continue;

                    int.TryParse(claim.Value, out _userId);
                    break;
                }
                return _userId;
            }
        }

        private string UserName
        {
            get
            {
                if (!string.IsNullOrEmpty(_userName)) return _userName;

                var userInfo = _masterDBContext().User.AsNoTracking().FirstOrDefault(u => u.UserId == _userId);
                if (userInfo != null)
                {
                    _userName = userInfo.UserName;
                }

                return _userName;
            }
        }

        public int SubsidiaryId
        {
            get
            {
                if (_subsidiaryId > 0)
                    return _subsidiaryId;

                foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                {
                    if (claim.Type != UserClaimConstants.SubsidiaryId)
                        continue;

                    int.TryParse(claim.Value, out _subsidiaryId);
                    break;
                }
                return _subsidiaryId;
            }
        }
        public int? TimeZoneOffset
        {
            get
            {
                if (_timeZoneOffset.HasValue)
                    return _timeZoneOffset.Value;
                var timeZoneOffsets = new StringValues();
                _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(Headers.TimeZoneOffset, out timeZoneOffsets);

                if (timeZoneOffsets.Count == 0 || !int.TryParse(timeZoneOffsets[0], out int timeZoneOffset))
                {
                    timeZoneOffset = 0;
                }
                return timeZoneOffset;
            }
        }
        public EnumActionType Action
        {
            get
            {
                if (_action.HasValue)
                {
                    return _action.Value;
                }

                var method = (EnumMethod)Enum.Parse(typeof(EnumMethod), _httpContextAccessor.HttpContext.Request.Method, true);

                if (_httpContextAccessor.HttpContext.Items.ContainsKey(HttpContextActionConstants.Action))
                {

                    _action = (EnumActionType)_httpContextAccessor.HttpContext.Items[HttpContextActionConstants.Action];
                }
                else
                {
                    _action = method.GetDefaultAction();
                }

                return _action.Value;
            }
        }

        public RoleInfo RoleInfo
        {
            get
            {
                if (_roleInfo != null)
                {
                    return _roleInfo;
                }
                if (UserId == 0)
                {
                    return null;
                }

                var masterDBContext = _masterDBContext();

                var userInfo = masterDBContext.User.AsNoTracking().First(u => u.UserId == UserId);
                var roleInfo = (
                    from r in masterDBContext.Role
                    where r.RoleId == userInfo.RoleId
                    select new
                    {
                        r.RoleId,
                        r.IsDataPermissionInheritOnStock,
                        r.IsModulePermissionInherit,
                        r.ChildrenRoleIds,
                        r.RoleName
                    }
                   )
                   .First();

                _roleInfo = new RoleInfo(
                    roleInfo.RoleId,
                    roleInfo.ChildrenRoleIds?.Split(',')?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList(),
                    roleInfo.IsDataPermissionInheritOnStock,
                    roleInfo.IsModulePermissionInherit,
                    roleInfo.RoleName
                );

                return _roleInfo;
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
                if (UserId == 0)
                {
                    return null;
                }
                var roleInfo = RoleInfo;

                var roleIds = new List<int>();
                roleIds.Add(roleInfo.RoleId);
                if (roleInfo.IsDataPermissionInheritOnStock && roleInfo.ChildrenRoleIds?.Count > 0)
                {
                    roleIds.AddRange(roleInfo.ChildrenRoleIds);
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var masterDBContext = scope.ServiceProvider.GetRequiredService<UnAuthorizeMasterDBContext>();

                    _stockIds = masterDBContext.RoleDataPermission
                    .Where(d => d.ObjectTypeId == (int)EnumObjectType.Stock && roleIds.Contains(d.RoleId))
                    .Select(d => d.ObjectId)
                    .ToList()
                    .Select(d => (int)d)
                    .ToArray();
                }

                return _stockIds;
            }
        }

        public bool IsDeveloper
        {
            get
            {

                var subdiaryInfo = _organizationDBContext().Subsidiary.FirstOrDefault(s => s.SubsidiaryId == SubsidiaryId);

                if (subdiaryInfo == null) return false;

                return _appSetting.Developer?.IsDeveloper(UserName, subdiaryInfo.SubsidiaryCode) == true;
            }
        }

        public string Language
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_language))
                    return _language;

                var languages = new StringValues();
                _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(Headers.Language, out languages);

                if (languages.Count == 0)
                {
                    return "";
                }
                _language = languages.ToString();
                return _language;
            }
        }
    }

    public class ScopeCurrentContextService : ICurrentContextService
    {
        public ScopeCurrentContextService(ICurrentContextService currentContextService)
        : this(
                currentContextService.UserId,
                currentContextService.Action,
                currentContextService.RoleInfo,
                currentContextService.StockIds,
                currentContextService.SubsidiaryId,
                currentContextService.TimeZoneOffset,
                currentContextService.Language
        )
        {

        }

        public ScopeCurrentContextService(int userId, EnumActionType action, RoleInfo roleInfo, IList<int> stockIds, int subsidiaryId, int? timeZoneOffset, string language = "vi-VN")
        {
            UserId = userId;
            SubsidiaryId = subsidiaryId;
            Action = action;
            RoleInfo = roleInfo == null ? null : roleInfo.JsonSerialize().JsonDeserialize<RoleInfo>();
            StockIds = stockIds == null ? null : stockIds.JsonSerialize().JsonDeserialize<List<int>>();
            TimeZoneOffset = timeZoneOffset;
            Language = language;
        }

        public void SetSubsidiaryId(int subsidiaryId)
        {
            SubsidiaryId = subsidiaryId;
        }

        public int UserId { get; } = 0;
        public int SubsidiaryId { get; private set; } = 0;
        public EnumActionType Action { get; }
        public IList<int> StockIds { get; }
        public RoleInfo RoleInfo { get; }
        public int? TimeZoneOffset { get; }
        public bool IsDeveloper { get; } = false;
        public string Language { get; } = "vi-VN";
    }
}
