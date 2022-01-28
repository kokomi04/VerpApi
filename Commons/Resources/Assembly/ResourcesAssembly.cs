using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace Verp.Resources
{
    public static class ResourcesAssembly
    {
        public static Assembly Assembly => typeof(ResourcesAssembly).Assembly;

        private static Dictionary<string, ResourceManager> _resources = new Dictionary<string, ResourceManager>();
        private static ResourceManager GetResouce(string resourceBase)
        {
            if (_resources.ContainsKey(resourceBase)) return _resources[resourceBase];

            var indexBaseName = resourceBase.LastIndexOf('.');
            var resourceBaseName = resourceBase.Substring(indexBaseName + 1);

            var type = Assembly.DefinedTypes.FirstOrDefault(t => t.FullName.Equals(resourceBase, StringComparison.OrdinalIgnoreCase));
            if (type == null)
            {
                type = Assembly.DefinedTypes.FirstOrDefault(t => t.Name.Equals(resourceBaseName, StringComparison.OrdinalIgnoreCase));
            }
            if (type == null)
            {
                _resources.Add(resourceBase, null);
                return null;
            }
            var resouceInfo = new ResourceManager(type);
            _resources.Add(resourceBase, resouceInfo);
            return resouceInfo;
        }

        public static string GetResouceString(string resourceName)
        {
            var lastIndx = resourceName.LastIndexOf('.');
            var resoucesBase = resourceName.Substring(0, lastIndx);
            var resourceManager = GetResouce(resoucesBase);
            if (resourceManager == null) return null;

            var resouceKey = resourceName.Substring(lastIndx + 1);
            return resourceManager.GetString(resouceKey, Thread.CurrentThread.CurrentCulture);
        }
    }
}
