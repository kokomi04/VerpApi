using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.MasterDB;
using VErpApi.Controllers.System.Config;

namespace MigrateAndMappingApi.Services
{

    public class ControllerMethod
    {
        public Type Controller { get; set; }
        public MethodInfo Method { get; set; }
    }

    public class DiscoverApiEndpointService
    {

        public void GetAllMethodsOfControler(Type controller, Type type, List<ControllerMethod> methods)
        {
            if (type == typeof(Microsoft.AspNetCore.Mvc.ControllerBase)
                || type == typeof(Microsoft.AspNetCore.Mvc.Controller)
                || type == typeof(VErpBaseController)
                )
            {
                return;
            }

            var lst = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.FlattenHierarchy).ToList();

            foreach (var item in lst)
            {
                if (!methods.Any(m => m.Method.Name == item.Name && m.Method.GetParameters().Length == m.Method.GetParameters().Length))
                {
                    methods.Add(new ControllerMethod()
                    {
                        Controller = controller,
                        Method = item
                    });
                }
            }
            if (type.BaseType != null)
            {
                GetAllMethodsOfControler(controller, type.BaseType, methods);
            }
        }

        public List<ApiEndpoint> GetActionsControllerFromAssenbly(Type assemblyType, int serviceId)
        {
            var assembly = assemblyType.Assembly;

            var lst = new List<ApiEndpoint>();
            var v1 = assembly.GetTypes();
            var v2 = v1
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(type)
                || typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(type)
                )
                .Where(type => !type.Name.StartsWith("Test") && !type.IsAbstract);

            var v3 = v2.SelectMany(type =>
            {
                var methods = new List<ControllerMethod>();
                GetAllMethodsOfControler(type, type, methods);
                return methods;
            }
            //type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.FlattenHierarchy)            
            );
            var v4 = v3.Where(m => !m.Method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any());
            var controllerActionList = v4.Where(x => !x.Method.IsAbstract)
                .Select(x => new
                {
                    Controller = x.Controller.Name.ToLower(),
                    IsMvcController = typeof(Microsoft.AspNetCore.Mvc.Controller).IsAssignableFrom(x.Controller),
                    ControllerAttributes = x.Controller.GetCustomAttributes(),
                    Action = x.Method.Name,
                    ReturnType = x.Method.ReturnType.Name,
                    Attributes = x.Method.GetCustomAttributes()
                }
            );


          

            /*discover api list*/
            foreach (var item in controllerActionList)
            {

                var endpoint = new ApiEndpoint()
                {
                    MethodId = (int)EnumMethod.Get,
                    ActionId = (int)EnumActionType.View,
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
