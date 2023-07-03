using System;

namespace VErp.Commons.Constants.Caching
{
    public static class ConfigCachingTtlConstants
    {
        public static readonly TimeSpan CONFIG_CACHING_TIMEOUT = TimeSpan.FromMinutes(10);

        public static readonly TimeSpan CONFIG_PRODUCTION_LONG_CACHING_TIMEOUT = TimeSpan.FromDays(1);

    }
}
