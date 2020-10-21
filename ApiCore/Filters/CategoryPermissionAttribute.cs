using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class CategoryPermissionAttribute : ActionFilterAttribute
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;

        public CategoryPermissionAttribute(IOptionsSnapshot<AppSetting> appSetting
           , ILogger<InternalCrossAuthorizeAttribute> logger
            , ICurrentContextService currentContextService
            , MasterDBContext masterDBContext)
        {
            _masterDBContext = masterDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _currentContextService = currentContextService;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var globalapi = context.ActionDescriptor.FilterDescriptors.FirstOrDefault(x => x.Filter is GlobalApiAttribute);
            if (globalapi != null)
            {
                return base.OnActionExecutionAsync(context, next);
            }

            if (context.RouteData.Values.ContainsKey("categoryId") && !context.HttpContext.Request.Method.Contains("GET"))
            {
                var roleInfo = _currentContextService.RoleInfo;
                int.TryParse(context.RouteData.Values["categoryId"].ToString(), out var categoryId);

                if (!AccessCategoryPermission(roleInfo.RoleId, categoryId))
                {
                    _logger.LogError($"CategoryPermissionAttribute: {categoryId}");
                    context.Result = new ForbidResult();
                    return Task.CompletedTask;
                }
            }
            return base.OnActionExecutionAsync(context, next);
        }

        private bool AccessCategoryPermission(int roleId, int categoryId)
        {
            return _masterDBContext.RoleDataPermission
                .Where(x => x.ObjectTypeId == (int)EnumObjectType.Category)
                .Any(r => r.RoleId == roleId && r.ObjectId == categoryId);
        }
    }
}
