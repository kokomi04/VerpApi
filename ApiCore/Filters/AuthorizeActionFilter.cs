using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class AuthorizeActionFilter : IAsyncActionFilter
    {
        private readonly AppSetting _appSetting;
        private readonly MasterDBContext _masterContext;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;

        public AuthorizeActionFilter(
           IOptionsSnapshot<AppSetting> appSetting
           , ILogger<AuthorizeActionFilter> logger
            , MasterDBContext masterContext
            , ICurrentContextService currentContextService
       )
        {
            _appSetting = appSetting.Value;
            _masterContext = masterContext;
            _logger = logger;
            _currentContextService = currentContextService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var allowAnonymousFilter = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is AllowAnonymousFilter || x.Filter is GlobalApiAttribute);
            if (allowAnonymousFilter != null)
            {
                await next();
                return;
            }

#if DEBUG
            await next();
            return;
#endif

            var headers = context.HttpContext.Request.Headers;
            var moduleIds = new StringValues();

            headers.TryGetValue(VerpHeaders.X_Module, out moduleIds);

            if (moduleIds.Count == 0)
            {
                var json = new ApiResponse
                {
                    Code = GeneralCode.X_ModuleMissing.GetErrorCodeString(),
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

            var apiEndpointId = Utils.HashApiEndpointId(route, methodId);

            var apiInfo = await _masterContext.ApiEndpoint.FirstOrDefaultAsync(a => a.ApiEndpointId == apiEndpointId);

            if (apiInfo == null)
            {
                var json = new ApiResponse
                {
                    Code = GeneralCode.Forbidden.GetErrorCodeString(),
                    Message = "api endpoint not found"
                };
                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var apiModuleMapped = await _masterContext.ModuleApiEndpointMapping.FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.ApiEndpointId == apiEndpointId);

            if (apiModuleMapped == null)
            {
                var json = new ApiResponse
                {
                    Code = GeneralCode.Forbidden.GetErrorCodeString(),
                    Message = "api endpoint is not mapped to module"
                };
                context.Result = new JsonResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var userId = _currentContextService.UserId;
            var roleInfo = _currentContextService.RoleInfo;
            var roleIds = new List<int>() { roleInfo.RoleId };

            if (roleInfo.IsModulePermissionInherit && roleInfo.ChildrenRoleIds?.Count > 0)
            {
                roleIds.AddRange(roleInfo.ChildrenRoleIds);
            }

            var lstPermission = (
                from p in _masterContext.RolePermission
                where p.ModuleId == moduleId
                && roleIds.Contains(p.RoleId)
                select p.Permission
                )
                .ToList();

            var permission = 0;
            if (lstPermission.Count > 0)
                permission = lstPermission.Aggregate((p1, p2) => p1 | p2);

            if ((permission & apiInfo.ActionId) == apiInfo.ActionId)
            {
                await next();
                return;
            }


            var data = new ApiResponse
            {
                Code = GeneralCode.Forbidden.GetErrorCodeString(),
                Message = GeneralCode.Forbidden.GetEnumDescription()
            };

            context.Result = new JsonResult(data);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }
    }
}
