using System.Reflection;

namespace ConfigApi
{
    public static class ConfigApiAssembly
    {
        public static Assembly Assembly => typeof(ConfigApiAssembly).Assembly;
        public const int ServiceId = 1;
    }
}
