using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class ResponseStatusFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            IActionResult result = context.Result;
            if(result is ObjectResult && !(context.Result as ObjectResult).StatusCode.HasValue)
            {
                var data = (result as ObjectResult).Value;
                if(data is ApiResponse)
                {
                    (context.Result as ObjectResult).StatusCode = (int)(data as ApiResponse).StatusCode;
                }
            }
        }
    }
}
