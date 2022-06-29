using System.Reflection;

namespace VErp.Commons.GlobalObject
{
    public static class GlobalObjectAssembly
    {
        public static Assembly Assembly => typeof(GlobalObjectAssembly).Assembly;

    }
}
