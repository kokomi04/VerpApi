using System.Reflection;

namespace VErp.WebApis.VErpApi
{
    public static class VErpApiAssembly
    {
        //test build
        public static Assembly Assembly => typeof(VErpApiAssembly).Assembly;

        public const int ServiceId = 0;
    }
}
