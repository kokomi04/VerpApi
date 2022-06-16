using System;

namespace Verp.Cache.RedisCache
{
    public class DistributedLockExeption : TimeoutException
    {
        public DistributedLockExeption(string resource)
            : base($"Get distrubuted lock '{resource}' timeout")
        {

        }
    }
}
