using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class DbContextExtensions
    {
        public static DbContextOptions<T> ChangeOptionsType<T>(this DbContextOptions options, ILoggerFactory loggerFactory) where T : DbContext
        {
            var sqlExt = options.Extensions.FirstOrDefault(e => e is SqlServerOptionsExtension);

            if (sqlExt == null)
                throw (new Exception("Failed to retrieve SQL connection string for base Context"));

            return new DbContextOptionsBuilder<T>()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                        .EnableSensitiveDataLogging(true)
                        .UseSqlServer(((SqlServerOptionsExtension)sqlExt).ConnectionString)
                        .UseLoggerFactory(loggerFactory)
                        .Options;
        }

        public static void RollbackEntities(this DbContext context)
        {
            var changedEntries = context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
