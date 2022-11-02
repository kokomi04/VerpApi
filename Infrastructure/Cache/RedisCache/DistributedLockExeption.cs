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

    public class LongTaskResourceLockException :Exception
    {
        public ILongTaskResourceInfo Info { get; }
        public LongTaskResourceLockException(ILongTaskResourceInfo info)
        {
            Info = info;
        }
    }
}
