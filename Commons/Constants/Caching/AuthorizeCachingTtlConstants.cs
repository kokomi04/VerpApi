using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Constants.Caching
{
    public static class AuthorizeCachingTtlConstants
    {
        public static readonly TimeSpan AUTHORIZED_CACHING_TIMEOUT = TimeSpan.FromMinutes(3);

        public static readonly TimeSpan AUTHORIZED_PRODUCTION_LONG_CACHING_TIMEOUT = TimeSpan.FromHours(1);

    }
}
