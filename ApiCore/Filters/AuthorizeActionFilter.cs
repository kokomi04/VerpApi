﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Commons.Constants.Caching.AuthorizeCacheKeys;
using static VErp.Commons.Constants.Caching.AuthorizeCachingTtlConstants;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class AuthorizeActionFilter : IAsyncActionFilter
    {
        private readonly AppSetting _appSetting;
        private readonly MasterDBContext _masterContext;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICachingService _cachingService;
        private readonly IAuthDataCacheService _authDataCacheService;

        public AuthorizeActionFilter(
           IOptionsSnapshot<AppSetting> appSetting
            , ILogger<AuthorizeActionFilter> logger
            , MasterDBContext masterContext
            , ICurrentContextService currentContextService
            , ICachingService cachingService
            , IAuthDataCacheService authDataCacheService
       )
        {
            _appSetting = appSetting.Value;
            _masterContext = masterContext;
            _logger = logger;
            _currentContextService = currentContextService;
            _cachingService = cachingService;
            _authDataCacheService = authDataCacheService;
        }



        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                await next();
                return;
            }
            var anonymous = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is AllowAnonymousAttribute);
            if (anonymous != null)
            {
                await next();
                return;
            }

            var ur = await _authDataCacheService.UserInfo(_currentContextService.UserId);

            if (ur.UserStatusId != (int)EnumUserStatus.Actived)
            {
                var json = new ServiceResult
                {
                    Code = GeneralCode.UserInActived,
                    Message = GeneralCode.UserInActived.GetEnumDescription()
                };

                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }


            var globalapi = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is GlobalApiAttribute);
            if (globalapi != null)
            {
                await next();
                return;
            }

#if DEBUG
            await next();
            return;
#endif

            //var headers = context.HttpContext.Request.Headers;
#pragma warning disable CS0162 // Unreachable code detected
            var moduleIds = new StringValues();
#pragma warning restore CS0162 // Unreachable code detected

            context.HttpContext.Request.Headers.TryGetValue(VerpHeaders.X_Module, out moduleIds);

            if (moduleIds.Count == 0)
            {
                var json = new ServiceResult
                {
                    Code = GeneralCode.X_ModuleMissing,
                    Message = GeneralCode.X_ModuleMissing.GetEnumDescription()
                };

                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
            var moduleId = int.Parse(moduleIds[0]);

            var route = context.ActionDescriptor.AttributeRouteInfo.Template;
            var method = context.HttpContext.Request.Method;
            var methodId = Enum.Parse<EnumMethod>(method, true);

            var apiEndpointId = Utils.HashApiEndpointId(_appSetting.ServiceId, route, methodId);

            (await _authDataCacheService.ApiEndpoints()).TryGetValue(apiEndpointId, out var apiInfo);

            if (apiInfo == null)
            {
                var json = new ServiceResult
                {
                    Code = GeneralCode.Forbidden,
                    Message = "api endpoint not found"
                };
                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }


            if ((await ModuleApiEndpointMappings(moduleId))?.Contains(apiEndpointId) != true)
            {
                var json = new ServiceResult
                {
                    Code = GeneralCode.Forbidden,
                    Message = "api endpoint is not mapped to module"
                };
                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }


            var (isValidateObject, objectTypeId, objectId, actionButtonId) = GetValidateObjectData(context);

            if (actionButtonId > 0 && objectTypeId.HasValue && objectId.HasValue)
            {

                if ((await _authDataCacheService.ActionButtons()).TryGetValue(actionButtonId, out var actionDatas))
                {
                    var actionInfo = actionDatas.FirstOrDefault(m => m.BillTypeObjectTypeId == objectTypeId.Value && m.BillTypeObjectId == (int)objectId.Value);
                    if (actionInfo != null)
                    {
                        apiInfo.ActionId = actionInfo.ActionType;
                    }
                }
            }


            var roleInfo = _currentContextService.RoleInfo;
            var permission = 0;
            if (!isValidateObject)
            {
                permission = await RoleModulePermission(roleInfo, moduleId);
            }
            else
            {
                permission = await RoleObjectPermission(roleInfo, objectTypeId.Value, objectId ?? 0);
            }

            if ((permission & apiInfo.ActionId) == apiInfo.ActionId)
            {
                await next();
                return;
            }


            var data = new ServiceResult
            {
                Code = GeneralCode.Forbidden,
                Message = GeneralCode.Forbidden.GetEnumDescription()
            };

            context.Result = new JsonResult(data);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }


        private (bool isValidateObject, EnumObjectType? objectTypeId, long? objectId, int actionButtonId) GetValidateObjectData(ActionExecutingContext context)
        {
            int actionButtonId = 0;

            var actionButtonAttr = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is ActionButtonDataApiAttribute)?.Filter as ActionButtonDataApiAttribute;
            if (actionButtonAttr != null)
            {
                context.RouteData.Values.TryGetValue(actionButtonAttr.RouterActionButtonIdKey, out var strActionButtonId);
                int.TryParse(strActionButtonId?.ToString(), out actionButtonId);
            }

            var method = context.HttpContext.Request.Method;
            var methodId = Enum.Parse<EnumMethod>(method, true);
            var action = methodId.GetDefaultAction();

            var actionAttr = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is VErpActionAttribute);
            if (actionAttr != null)
            {
                action = (actionAttr.Filter as VErpActionAttribute).Action;
            }

            var objectApi = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is ObjectDataApiAttribute)?.Filter as ObjectDataApiAttribute;
            if (objectApi == null) return (false, null, null, actionButtonId);


            if (context.RouteData.Values.ContainsKey(objectApi.RouterDataKey))// && action != EnumAction.View)
            {
                long.TryParse(context.RouteData.Values[objectApi.RouterDataKey].ToString(), out var objectId);

                return (true, objectApi.ObjectTypeId, objectId, actionButtonId);
            }

            return (false, null, null, actionButtonId);
        }

        private async Task<int> RoleObjectPermission(RoleInfo role, EnumObjectType objectTypeId, long objectId)
        {
            return await TryGetSet(RoleObjectPermissionCacheKey(role.RoleId, objectTypeId, objectId), async () =>
            {
                var roleIds = GetInheritRoleIds(role);
                var lstPermissions = await _masterContext.RolePermission.Where(p => roleIds.Contains(p.RoleId) && p.ObjectTypeId == (int)objectTypeId && p.ObjectId == objectId).Select(p => p.Permission).ToListAsync();
                return AggregatePermission(lstPermissions);
            });
        }


        private async Task<int> RoleModulePermission(RoleInfo role, int moduleId)
        {
            return await TryGetSet(RoleModulePermissionCacheKey(role.RoleId, moduleId), async () =>
            {
                var roleIds = GetInheritRoleIds(role);
                var lstPermissions = await _masterContext.RolePermission.Where(p => roleIds.Contains(p.RoleId) && p.ModuleId == moduleId).Select(p => p.Permission).ToListAsync();
                return AggregatePermission(lstPermissions);
            });
        }

        private IList<int> GetInheritRoleIds(RoleInfo roleInfo)
        {
            var roleIds = new List<int>() { roleInfo.RoleId };

            if (roleInfo.IsModulePermissionInherit && roleInfo.ChildrenRoleIds?.Count > 0)
            {
                roleIds.AddRange(roleInfo.ChildrenRoleIds);
            }
            return roleIds;
        }

        private int AggregatePermission(IList<int> lstPermission)
        {
            var permission = 0;
            if (lstPermission?.Count > 0)
                permission = lstPermission.Aggregate((p1, p2) => p1 | p2);
            return permission;
        }


        private async Task<HashSet<Guid>> ModuleApiEndpointMappings(int moduleId)
        {
            var t = AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT;
            if (!EnviromentConfig.IsProduction)
            {
                t = AUTHORIZED_CACHING_TIMEOUT;
            }
            return await _cachingService.TryGetSet(AUTH_TAG, ModuleApiEndpointMappingsCacheKey(moduleId), t, async () =>
            {
                return (await _masterContext.ModuleApiEndpointMapping.AsNoTracking().Where(m => m.ModuleId == moduleId).Select(p => p.ApiEndpointId).ToListAsync()).ToHashSet();
            });
        }


        private Task<T> TryGetSet<T>(string key, Func<Task<T>> queryData)
        {
            return _cachingService.TryGetSet(AUTH_TAG, key, AUTHORIZED_CACHING_TIMEOUT, queryData);
        }
    }
}
