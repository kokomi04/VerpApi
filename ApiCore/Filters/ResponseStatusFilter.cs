using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            if (result is ObjectResult && !(context.Result as ObjectResult).StatusCode.HasValue)
            {
                var data = (result as ObjectResult).Value;
                if (data is ApiResponse)
                {
                    int statusCode = (int)(data as ApiResponse).StatusCode;
                    (context.Result as ObjectResult).StatusCode = statusCode;
                    if (statusCode == (int)HttpStatusCode.OK)
                    {
                        (data as ApiResponse).Code = null;
                        (data as ApiResponse).Message = null;
                        (context.Result as ObjectResult).Value = data;
                    }
                }

            }
        }
    }
}
