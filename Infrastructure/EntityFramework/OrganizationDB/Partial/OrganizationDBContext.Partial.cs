using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter("IsDeleted", isDeleted);
                }

                entityType.SetQueryFilter(filterBuilder.Build());
            }

        }
    }


}
