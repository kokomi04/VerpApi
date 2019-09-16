using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.ApiCore.Filters
{
    // IOperationFilter implementation that will validate whether an action has an applicable Authorize attribute.
    // If it does, we add the VerpApi scope so IdentityServer can validate permission for that scope.
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                               context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) return;

            operation.Responses.TryAdd("401", new Response { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new Response { Description = "Forbidden" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new [] { "VerpApi" } },
                    { "Bearer", new string[] { } },
                }
            };
        }
    }

    public class DataSchemaFilter : ISchemaFilter
    {

        public void Apply(Schema schema, SchemaFilterContext context)
        {

            if (Nullable.GetUnderlyingType(context.SystemType)?.IsEnum == true)
            {
                var example = new Dictionary<string, int>();
                var lst = new List<object>();
                foreach (var item in Enum.GetValues(Nullable.GetUnderlyingType(context.SystemType)))
                {
                    example.Add(item.ToString(), (int)item);
                    lst.Add(item.ToString() + ": " + (int)item);
                }
                schema.Enum = lst;
            }

            if (context.SystemType.IsEnum)
            {
                var example = new Dictionary<string, int>();
                var lst = new List<object>();
                foreach (var item in Enum.GetValues(context.SystemType))
                {
                    example.Add(item.ToString(), (int)item);
                    lst.Add(item.ToString() + ": " + (int)item);
                }
                schema.Enum = lst;
            }

            if (Nullable.GetUnderlyingType(context.SystemType) != null)
            {
                schema.Type = $"Nullable<{schema.Type}>";
            }

        }
    }
}
