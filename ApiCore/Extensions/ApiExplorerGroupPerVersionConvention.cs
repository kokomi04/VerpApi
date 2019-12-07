using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.ApiCore.Extensions
{
    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace;
            var ns = controllerNamespace.Split('.').ToArray();
            if (ns.Length > 2)
            {
                var apiVersion = ns[2].ToLower();
                controller.ApiExplorer.GroupName = apiVersion;
            }
        }
    }
}
