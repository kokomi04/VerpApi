using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace VErp.WebApis.VErpApi
{
    public class VErpApiAssembly
    {
        public static Assembly Assembly => typeof(VErpApiAssembly).Assembly;
    }
}
