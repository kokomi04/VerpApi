using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class VErpActionAttribute : ActionFilterAttribute
    {
        public EnumActionType Action { get; set; }
        public VErpActionAttribute(EnumActionType action)
        {
            Action = action;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Items.ContainsKey(HttpContextActionConstants.Action))
            {
                context.HttpContext.Items.Add(HttpContextActionConstants.Action, Action);
            }
            base.OnActionExecuting(context);
        }
    }
}
