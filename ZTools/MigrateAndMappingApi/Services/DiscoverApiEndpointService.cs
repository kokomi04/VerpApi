using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.MasterDB;

namespace MigrateAndMappingApi.Services
{
    public class DiscoverApiEndpointService
    {
        public List<ApiEndpoint> GetActionsControllerFromAssenbly(Type assemblyType, int serviceId)
        {
            var assembly = assemblyType.Assembly;

            var lst = new List<ApiEndpoint>();
            var v1 = assembly.GetTypes();
            var v2 = v1
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(type)
                || typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(type)
                )
                .Where(type => !type.Name.StartsWith("Test"));

            var v3 = v2.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public));
            var v4 = v3.Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any());
            var controllerActionList = v4.Select(x => new
            {
                Controller = x.DeclaringType.Name.ToLower(),
                IsMvcController = typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(x.DeclaringType),
                ControllerAttributes = x.DeclaringType.GetCustomAttributes(),
                Action = x.Name,
                ReturnType = x.ReturnType.Name,
                Attributes = x.GetCustomAttributes(),
            }
            );

            /*discover api list*/
            foreach (var item in controllerActionList)
            {
                var endpoint = new ApiEndpoint()
                {
                    MethodId = (int)EnumMethod.Get,
                    ActionId = (int)EnumAction.View,
                    ServiceId = serviceId
                };

                var controllerRoute = "";

                foreach (Attribute attribute in item.ControllerAttributes)
                {
                    if (attribute is RouteAttribute)
                    {
                        var route = (RouteAttribute)attribute;
                        var template = route.Template;
                        controllerRoute = (template ?? "").Replace("[controller]", item.Controller.Substring(0, item.Controller.LastIndexOf("controller")));
                    }
                }
                if (string.IsNullOrWhiteSpace(controllerRoute))
                {
                    controllerRoute = item.Controller.Substring(0, item.Controller.LastIndexOf("controller"));
                    if (item.IsMvcController)
                    {
                        controllerRoute += "/" + item.Action;
                    }
                }


                bool isCustomAction = false;
                foreach (Attribute attribute in item.Attributes)
                {
                    if (attribute is HttpGetAttribute)
                    {
                        endpoint.MethodId = (int)EnumMethod.Get;

                        controllerRoute = GetRouteTemplateFromHttpMethodAttr(attribute, controllerRoute);

                    }
                    else if (attribute is HttpPostAttribute)
                    {
                        endpoint.MethodId = (int)EnumMethod.Post;

                        controllerRoute = GetRouteTemplateFromHttpMethodAttr(attribute, controllerRoute);
                    }
                    else if (attribute is HttpPutAttribute)
                    {
                        endpoint.MethodId = (int)EnumMethod.Put;

                        controllerRoute = GetRouteTemplateFromHttpMethodAttr(attribute, controllerRoute);
                    }
                    else if (attribute is HttpPatchAttribute)
                    {
                        endpoint.MethodId = (int)EnumMethod.Patch;

                        controllerRoute = GetRouteTemplateFromHttpMethodAttr(attribute, controllerRoute);
                    }
                    else if (attribute is HttpDeleteAttribute)
                    {
                        endpoint.MethodId = (int)EnumMethod.Delete;

                        controllerRoute = GetRouteTemplateFromHttpMethodAttr(attribute, controllerRoute);
                    }
                    else if (attribute is RouteAttribute)
                    {
                        var route = (RouteAttribute)attribute;
                        if (!string.IsNullOrWhiteSpace(route.Template))
                        {
                            controllerRoute = controllerRoute + "/" + route.Template;
                        }
                    }

                    if (attribute is VErpActionAttribute)
                    {
                        endpoint.ActionId = (int)(attribute as VErpActionAttribute).Action;
                        isCustomAction = true;
                    }
                }

                if (!isCustomAction)
                {
                    endpoint.ActionId = (int)((EnumMethod)endpoint.MethodId).GetDefaultAction();
                }

                endpoint.Route = controllerRoute;

                lst.Add(endpoint);
            }

            return lst;
        }

        private static string GetRouteTemplateFromHttpMethodAttr(Attribute attribute, string controllerRoute)
        {
            var route = (HttpMethodAttribute)attribute;
            if (!string.IsNullOrWhiteSpace(route.Template))
            {
                return controllerRoute + "/" + route.Template;
            }
            return controllerRoute;
        }
    }
}
