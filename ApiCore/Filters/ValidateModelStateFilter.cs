using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;

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
                    var invalidParams = new ServiceResult()
                    {
                        Code = GeneralCode.InvalidParams,
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


            var invalidModels = new ServiceResult()
            {
                Code = GeneralCode.InvalidParams,
                Message = string.Join(",", validationErrors)
            };

            context.Result = new BadRequestObjectResult(invalidModels);
        }

        private Enum ValidateEnum(dynamic objs)
        {
            foreach (object obj in objs)
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
                    if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        if (!ValidateEnum(obj).IsSuccess())
                        {
                            return GeneralCode.InvalidParams;
                        }
                    }
                    else
                    {
                        if (type.IsClass && !(obj is Microsoft.AspNetCore.Http.HeaderDictionary))
                        {
                            foreach (var p in type.GetProperties())
                            {

                                var v = p.GetValue(obj);

                                if (v != null)
                                {
                                    var vType = v.GetType();
                                    if (vType == typeof(string))
                                    {
                                        try
                                        {
                                            p.SetValue(obj, v.ToString().Trim());
                                        }
                                        catch (Exception)
                                        {


                                        }

                                    }
                                }
                                if (!ValidateEnum(new List<object>() { v }).IsSuccess())
                                {
                                    return GeneralCode.InvalidParams;
                                }
                            }
                        }
                    }
                }
            }
            return GeneralCode.Success;
        }
    }
}
