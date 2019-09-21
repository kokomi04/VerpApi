using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                if (!ValidateEnum(context.ActionArguments.Select(a => a.Value)).IsSuccess())
                {
                    var invalidParams = new ApiResponse()
                    {
                        Code = GeneralCode.InvalidParams.GetErrorCodeString(),
                        Message = GeneralCode.InvalidParams.GetEnumDescription()
                    };

                    context.Result = new BadRequestObjectResult(invalidParams);
                    return;
                }

                return;
            }

            var validationErrors = context.ModelState
                .Keys
                .SelectMany(k => context.ModelState[k].Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();

            var json = new JsonErrorResponse
            {
                Messages = validationErrors
            };

            context.Result = new BadRequestObjectResult(json);
        }

        private Enum ValidateEnum(IEnumerable<object> objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    continue;
                }

                var type = obj.GetType();
                bool isPrimitiveType = type.IsPrimitive || type.IsValueType || (type == typeof(string));

                if (isPrimitiveType)
                {
                    if (type.IsEnum)
                    {
                        if (!type.IsEnumDefined(obj))
                        {
                            return GeneralCode.InvalidParams;
                        }
                    }
                }
                else
                {
                    if (type.IsArray || type.IsGenericType)
                    {
                        if (!ValidateEnum((IEnumerable<object>)obj).IsSuccess())
                        {
                            return GeneralCode.InvalidParams;
                        }
                    }
                    else
                    {
                        foreach (var p in type.GetProperties())
                        {
                            var v = p.GetValue(obj);
                            if (!ValidateEnum(new List<object>() { v }).IsSuccess())
                            {
                                return GeneralCode.InvalidParams;
                            }
                        }
                    }
                }
            }
            return GeneralCode.Success;
        }
    }
}
