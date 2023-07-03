using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Constants.Caching
{
    public static class ConfigCacheKeys
    {
        public static string CONFIG_TAG = "CONFIG_TAG";
        public static string ConfigCacheKey(string configName)
        {
            return $"CONFIG_{configName}";
        }
    }
}
