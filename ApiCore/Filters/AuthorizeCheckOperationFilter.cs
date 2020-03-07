using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore.Model;

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

            operation.Responses.TryAdd("401", new Response { Description = "Unauthorized - Chưa đăng nhập" });
            operation.Responses.TryAdd("403", new Response { Description = "Forbidden - Không có quyền truy cập module" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new [] { "VerpApi" } },
                    { "Bearer", new string[] { } },
                }
            };

            operation.Produces = new List<string>() { "application/json" };
            operation.Consumes = new List<string>() { "application/json" };
        }
    }

    public class DataSchemaFilter : ISchemaFilter
    {

        public void Apply(Schema schema, SchemaFilterContext context)
        {

            //if (Nullable.GetUnderlyingType(context.SystemType)?.IsEnum == true)
            //{
            //    var lst = new List<object>();
            //    object example = null;
            //    foreach (var item in Enum.GetValues(Nullable.GetUnderlyingType(context.SystemType)))
            //    {
            //        if (example == null)
            //        {
            //            example = item;
            //        }
            //        lst.Add(item.ToString() + ": " + (int)item);
            //    }
            //    schema.Example = example;
            //    schema.Enum = lst;
            //}

            //if (context.SystemType.IsEnum)
            //{
            //    var lst = new List<object>();

            //    var prefix = context.SystemType.GetErrorCodePrefix(false);

            //    object example = null;
            //    foreach (var item in Enum.GetValues(context.SystemType))
            //    {
            //        if (example == null)
            //        {
            //            example = item;
            //        }

            //        if (string.IsNullOrWhiteSpace(prefix))
            //        {
            //            lst.Add(item.ToString() + ": " + (int)item);
            //        }
            //        else
            //        {
            //            lst.Add($"{item}: \"{prefix}-{(int)item}\"");
            //        }
            //    }
            //    schema.Example = example;
            //    schema.Enum = lst;
            //}

            if (Nullable.GetUnderlyingType(context.SystemType) != null)
            {
                schema.Description = "Nullable" + schema.Description;
            }


            var type = context.SystemType;

            var propertyMappings = type
           .GetProperties()
           .Join(
               schema.Properties ?? new Dictionary<string, Schema>(),
               x => x.Name.ToLower(),
               x => x.Key.ToLower(),
               (x, y) => new KeyValuePair<PropertyInfo, KeyValuePair<string, Schema>>(x, y))
           .ToList();

            foreach (var propertyMapping in propertyMappings)
            {
                var sc = propertyMapping.Value;

                if (propertyMapping.Key.PropertyType.IsEnum)
                {
                    //sc.Value.Ref = $"#/definitions/{propertyMapping.Key.PropertyType.Name}";
                    sc.Value.Description = $"{propertyMapping.Key.PropertyType.Name}";
                }


                if (Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType)?.IsEnum == true)
                {
                    //sc.Value.Ref = $"#/definitions/{Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType).Name}";
                    var t = Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType).Name;
                    sc.Value.Description = $"Nullable {t}";
                }

            }
        }

    }

    public class CustomModelDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var types = typeof(GeneralCode)
                .Assembly
                .GetTypes()
                .Where(t => t.IsEnum)
                .ToArray();

            foreach (var enumType in types)
            {
                var prefix = enumType.GetErrorCodePrefix(false);
                var sc = context.SchemaRegistry.GetOrRegister(enumType);

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    swaggerDoc.Definitions.Add($"{enumType.Name} ({prefix}-)", sc);
                }
                else
                {
                    swaggerDoc.Definitions.Add($"{enumType.Name}", sc);
                }
            }

        }
    }
}
