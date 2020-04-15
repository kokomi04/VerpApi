using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class AccountingDBContext
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

        public override int SaveChanges()
        {
            SetBaseValue();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetBaseValue();
            return await base.SaveChangesAsync(true, cancellationToken);
        }

        public void SetBaseValue()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedDatetimeUtc = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedDatetimeUtc = DateTime.UtcNow;
                }
            }
        }
    }

}
