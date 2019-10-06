using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class VErpActionAttribute : ActionFilterAttribute
    {
        public EnumAction Action { get; set; }
        public VErpActionAttribute(EnumAction action)
        {
            Action = action;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Items.ContainsKey("action"))
            {
                context.HttpContext.Items.Add("action", Action);
            }
            base.OnActionExecuting(context);
        }
    }
}
