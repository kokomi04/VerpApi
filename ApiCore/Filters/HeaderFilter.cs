using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Constants;

namespace VErp.Infrastructure.ApiCore.Filters
{
    public class HeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = Headers.Module,
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema()
                {
                    Type = "integer",
                }
            });

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = Headers.Language,
                In = ParameterLocation.Header,
                Required = false,                
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                    Default = new OpenApiString("vi-VN"),
                    Enum = new List<IOpenApiAny>()
                    {
                       new OpenApiString("vi-VN"),
                       new OpenApiString("en-US"),
                    }
                },

            });
        }
    }
}
