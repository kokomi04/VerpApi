using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class ResponseStatusFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            IActionResult result = context.Result;
            if (result is ObjectResult && !(context.Result as ObjectResult).StatusCode.HasValue)
            {
                if ((result as ObjectResult).Value is ServiceResult)
                {
                    var objectValue = (result as ObjectResult).Value as ServiceResult;
                    HttpStatusCode statusCode;
                    Enum code = objectValue.Code;
                    switch (code)
                    {
                        case GeneralCode.Success:
                            statusCode = HttpStatusCode.OK;
                            break;
                        case GeneralCode.InternalError:
                            statusCode = HttpStatusCode.InternalServerError;
                            break;
                        case GeneralCode.Forbidden:
                        case GeneralCode.X_ModuleMissing:
                        case GeneralCode.NotYetSupported:
                        case GeneralCode.DistributedLockExeption:
                            statusCode = HttpStatusCode.Forbidden;
                            break;
                        case GeneralCode.InvalidParams:
                        default:
                            statusCode = HttpStatusCode.BadRequest;
                            break;
                    }
                    (context.Result as ObjectResult).StatusCode = (int)statusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        var data = objectValue.GetType().GetProperty("Data")?.GetValue(objectValue) ?? true;
                        if (data is MemoryStream)
                        {
                            context.Result = new FileStreamResult(data as MemoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                FileDownloadName = "data.xlsx"
                            };
                        }
                        else
                        {
                            (context.Result as ObjectResult).Value = data;
                        }
                    }
                    else
                    {
                        (context.Result as ObjectResult).Value = new ApiErrorResponse
                        {
                            Code = code.GetErrorCodeString(),
                            Message = objectValue.Message ?? code.GetErrorCodeString()
                        };
                    }
                }

            }
        }
    }
}
