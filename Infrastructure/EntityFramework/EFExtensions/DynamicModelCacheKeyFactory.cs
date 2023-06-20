using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public interface IDbContextFilterTypeCache
    {
        bool IgnoreFilterSubsidiary { get; }
        bool IgnoreFilterStock { get; }
    }

    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {       

        public object Create(DbContext context, bool designTime)
        {
            if (context is IDbContextFilterTypeCache dynamicContext)
            {
                return (context.GetType(), dynamicContext.IgnoreFilterSubsidiary, dynamicContext.IgnoreFilterStock);
            }
            return context.GetType();
        }
    }
}
