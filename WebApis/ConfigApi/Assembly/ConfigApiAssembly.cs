using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConfigApi
{
    public static class ConfigApiAssembly
    {
        public static Assembly Assembly => typeof(ConfigApiAssembly).Assembly;
        public const int ServiceId = 1;
    }
}
