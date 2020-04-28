using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

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


        public static void SetHistoryBaseValue(this DbContext context, ICurrentContextService currentContext)
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                var obj = entityEntry.Entity;

                obj.SetValue("UpdatedByUserId", currentContext.UserId);

                if (entityEntry.State == EntityState.Added)
                {

                    obj.SetValue("CreatedDatetimeUtc", DateTime.UtcNow);
                    obj.SetValue("CreatedByUserId", currentContext.UserId);
                    obj.SetValue("IsDeleted", false);

                    obj.SetValue("UpdatedDatetimeUtc", DateTime.UtcNow);
                    obj.SetValue("UpdatedByUserId", currentContext.UserId);

                    obj.SetValue("DeletedDatetimeUtc", null);
                }
                else
                {
                    var isDeleted = obj.GetValue("IsDeleted") == (object)true;
                    if (isDeleted)
                    {
                        obj.SetValue("DeletedDatetimeUtc", DateTime.UtcNow);
                    }
                    else
                    {
                        obj.SetValue("UpdatedDatetimeUtc", DateTime.UtcNow);
                    }
                }
            }
        }

        private static void SetValue(this object obj, string property, object value)
        {
            var type = obj.GetType();
            var p = type.GetProperty(property);
            if (p != null)
            {
                p.SetValue(obj, value);
            }
        }

        private static object GetValue(this object obj, string property)
        {
            var type = obj.GetType();
            var p = type.GetProperty(property);
            if (p != null)
            {
                return p.GetValue(obj);
            }

            return null;
        }
    }
}
