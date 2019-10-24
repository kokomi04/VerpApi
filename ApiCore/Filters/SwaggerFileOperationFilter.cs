using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VErp.Infrastructure.ApiCore.Filters
{

    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType.FullName.Equals(typeof(Microsoft.AspNetCore.Http.IFormFile).FullName));
            var isUploadFile = fileParams.Any() && fileParams.Count() == 1;
            if (operation.Parameters != null)
            {
                var lst = new List<IParameter>();
                foreach (var p in operation.Parameters)
                {
                    if (p is NonBodyParameter)
                    {
                        var p1 = (p as NonBodyParameter);
                        if (p1.Enum != null)
                        {
                            for (int e = 0; e < p1.Enum.Count; e++)
                            {
                                p1.Enum[e] = p1.Enum[e].ToString().Split(':')[0];
                            }
                            p1.Type = "string";
                        }


                        if (isUploadFile)
                        {
                            if (p1.In != "formData")
                            {
                                lst.Add(p1);
                            }
                        }
                        else
                        {
                            lst.Add(p1);
                        }
                    }
                    else
                    {
                        lst.Add(p);
                    }
                }

                if (isUploadFile)
                {
                    foreach (var f in fileParams)
                    {
                        lst.Add(new NonBodyParameter
                        {
                            Name = f.Name,
                            Required = true,
                            Type = "file",
                            In = "formData"
                        });
                    }
                }
                operation.Parameters = lst;
            }
        }
    }
}
