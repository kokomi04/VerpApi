using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
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
        //public void Apply(Operation operation, OperationFilterContext context)
        //{
        //    var fileParams = context.MethodInfo.GetParameters()
        //        .Where(p => p.ParameterType.FullName.Equals(typeof(Microsoft.AspNetCore.Http.IFormFile).FullName));
        //    var isUploadFile = fileParams.Any() && fileParams.Count() == 1;
        //    if (operation.Parameters != null)
        //    {
        //        var lst = new List<IParameter>();
        //        foreach (var p in operation.Parameters)
        //        {
        //            if (p is NonBodyParameter)
        //            {
        //                var p1 = (p as NonBodyParameter);
        //                if (p1.Enum != null)
        //                {
        //                    for (int e = 0; e < p1.Enum.Count; e++)
        //                    {
        //                        p1.Enum[e] = p1.Enum[e].ToString().Split(':')[0];
        //                    }
        //                    p1.Type = "string";
        //                }


        //                if (isUploadFile)
        //                {
        //                    if (p1.In != "formData")
        //                    {
        //                        lst.Add(p1);
        //                    }
        //                }
        //                else
        //                {
        //                    lst.Add(p1);
        //                }
        //            }
        //            else
        //            {
        //                lst.Add(p);
        //            }
        //        }

        //        if (isUploadFile)
        //        {
        //            foreach (var f in fileParams)
        //            {
        //                lst.Add(new NonBodyParameter
        //                {
        //                    Name = f.Name,
        //                    Required = true,
        //                    Type = "file",
        //                    In = "formData"
        //                });
        //            }
        //        }
        //        operation.Parameters = lst;
        //    }
        //}

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType.FullName.Equals(typeof(Microsoft.AspNetCore.Http.IFormFile).FullName));

            var otherParams = context.MethodInfo.GetParameters()
               .Where(p => !p.ParameterType.FullName.Equals(typeof(Microsoft.AspNetCore.Http.IFormFile).FullName));

            var isUploadFile = fileParams.Count() > 0;
            if (isUploadFile)
            {
                operation.RequestBody.Content.Remove("multipart/form-data");
                if (isUploadFile)
                {
                    var paramsData = new Dictionary<string, OpenApiSchema>();
                    foreach (var f in fileParams)
                    {
                        paramsData.Add(f.Name, new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                        });
                    }

                    foreach (var f in otherParams)
                    {
                        var myObjectSchema = context.SchemaGenerator.GenerateSchema(f.ParameterType, context.SchemaRepository);

                        paramsData.Add(f.Name, myObjectSchema);
                    }



                    operation.RequestBody.Content.Add("multipart/form-data", new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = paramsData
                        }
                    });
                }
            }
        }
    }
}
