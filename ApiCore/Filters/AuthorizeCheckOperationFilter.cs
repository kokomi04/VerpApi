﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace VErp.Infrastructure.ApiCore.Filters
{
    // IOperationFilter implementation that will validate whether an action has an applicable Authorize attribute.
    // If it does, we add the VerpApi scope so IdentityServer can validate permission for that scope.
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                               context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) return;

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized - Chưa đăng nhập" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden - Không có quyền truy cập module" });
        }
    }

    //public class DataSchemaFilter : ISchemaFilter
    //{

    //    public void Apply(Schema schema, SchemaFilterContext context)
    //    {

    //        if (Nullable.GetUnderlyingType(context.SystemType)?.IsEnum == true)
    //        {
    //            var lst = new List<object>();
    //            object example = null;
    //            foreach (var item in Enum.GetValues(Nullable.GetUnderlyingType(context.SystemType)))
    //            {
    //                if (example == null)
    //                {
    //                    example = item;
    //                }
    //                lst.Add(item.ToString() + ": " + (int)item);
    //            }
    //            schema.Example = example;
    //            schema.Enum = lst;
    //        }

    //        if (context.SystemType.IsEnum)
    //        {
    //            var lst = new List<object>();

    //            var prefix = context.SystemType.GetErrorCodePrefix(false);

    //            object example = null;
    //            foreach (var item in Enum.GetValues(context.SystemType))
    //            {
    //                if (example == null)
    //                {
    //                    example = item;
    //                }

    //                if (string.IsNullOrWhiteSpace(prefix))
    //                {
    //                    lst.Add(item.ToString() + ": " + (int)item);
    //                }
    //                else
    //                {
    //                    lst.Add($"{item}: \"{prefix}-{(int)item}\"");
    //                }
    //            }
    //            schema.Example = example;
    //            schema.Enum = lst;
    //        }

    //        if (Nullable.GetUnderlyingType(context.SystemType) != null)
    //        {
    //            schema.Description = "Nullable" + schema.Description;
    //        }


    //        var type = context.SystemType;

    //        var propertyMappings = type
    //       .GetProperties()
    //       .Join(
    //           schema.Properties ?? new Dictionary<string, Schema>(),
    //           x => x.Name.ToLower(),
    //           x => x.Key.ToLower(),
    //           (x, y) => new KeyValuePair<PropertyInfo, KeyValuePair<string, Schema>>(x, y))
    //       .ToList();

    //        foreach (var propertyMapping in propertyMappings)
    //        {
    //            var sc = propertyMapping.Value;

    //            if (propertyMapping.Key.PropertyType.IsEnum)
    //            {
    //                //sc.Value.Ref = $"#/definitions/{propertyMapping.Key.PropertyType.Name}";
    //                sc.Value.Description = $"{propertyMapping.Key.PropertyType.Name}";
    //            }


    //            if (Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType)?.IsEnum == true)
    //            {
    //                //sc.Value.Ref = $"#/definitions/{Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType).Name}";
    //                var t = Nullable.GetUnderlyingType(propertyMapping.Key.PropertyType).Name;
    //                sc.Value.Description = $"Nullable {t}";
    //            }

    //        }
    //    }

    //}

    //public class CustomModelDocumentFilter : IDocumentFilter
    //{
    //    public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
    //    {
    //        var types = typeof(GeneralCode)
    //            .Assembly
    //            .GetTypes()
    //            .Where(t => t.IsEnum)
    //            .ToArray();

    //        foreach (var enumType in types)
    //        {
    //            var prefix = enumType.GetErrorCodePrefix(false);
    //            var sc = context.SchemaRegistry.GetOrRegister(enumType);

    //            if (!string.IsNullOrWhiteSpace(prefix))
    //            {
    //                swaggerDoc.Definitions.Add($"{enumType.Name} ({prefix}-)", sc);
    //            }
    //            else
    //            {
    //                swaggerDoc.Definitions.Add($"{enumType.Name}", sc);
    //            }
    //        }

    //    }
    //    class OpenApiAny : IOpenApiAny
    //    {
    //        string value;
    //        string name;
    //        public OpenApiAny(string name, string value)
    //        {
    //            this.name = name;
    //            this.value = value;
    //        }
    //        public AnyType AnyType => AnyType.Primitive;

    //        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    //        {
    //            writer.WriteProperty(this.name, this.value.ToString());
    //        }
    //    }
    //    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    //    {
    //        var types = typeof(GeneralCode)
    //          .Assembly
    //          .GetTypes()
    //          .Where(t => t.IsEnum)
    //          .ToArray();

    //        var repository = new SchemaRepository();
    //        var schema = new OpenApiSchema();
    //        var enums = new List<OpenApiAny>();


    //        foreach (var enumType in types)
    //        {
    //            var prefix = enumType.GetErrorCodePrefix(false);

    //           // var sc = context.SchemaGenerator.GenerateSchema(enumType, new SchemaRepository());



    //            schema.Enum = new List<OpenApiAny>()
    //            {

    //            }.Cast<IOpenApiAny>().ToList();

    //            schema.OneOf = true;
    //            var sc = new List<OpenApiSchema>();
    //            sc.Add(new OpenApiSchema()
    //            {
    //                Title= $"{enumType.Name} ({prefix}-)",

    //            })

    //            if (!string.IsNullOrWhiteSpace(prefix))
    //            {
    //                enums.Add(new OpenApiAny($"{enumType.Name}",prefix ))
    //            }
    //            else
    //            {
    //                enums.Add(new OpenApiAny($"{enumType.Name}", ""))
    //            }
    //        }
    //        schema.Enum = new List<OpenApiAny>()
    //        {

    //        }.Cast<IOpenApiAny>().ToList();

    //        repository.Ge tOrAdd(enumType, enumType.Name, () => { return new OpenApiSchema() { }; });
    //        context.SchemaGenerator.GenerateSchema(typeof(string), new SchemaRepository());
    //    }
    //}
}
