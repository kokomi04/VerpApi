using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public static void AddFilterBase(this ModelBuilder modelBuilder)
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
                    var isDeleted = obj.GetValue("IsDeleted").Equals((object)true);
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


        /// <summary>
        /// DynamicSelectGenerator
        /// Ref docs: https://stackoverflow.com/questions/42820866/dynamically-build-iqueryable-select-clause-with-fieldnames-in-net-core
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="fieldFroms"></param>
        /// <returns></returns>
        public static IQueryable<TResult> DynamicSelectGenerator<T, TResult>(this IQueryable<T> query, Dictionary<string, string> fieldFroms)
        {
            Dictionary<string, string> EntityFields;
            if (fieldFroms.Count == 0)
                // get Properties of the T
                EntityFields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray().ToDictionary(f => f, f => f);
            else
                EntityFields = fieldFroms;

            // input parameter "o"
            var xParameter = Expression.Parameter(typeof(T), "o");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(TResult));

            // create initializers
            var bindings = EntityFields
                .Select(o =>
                {
                    // property "Field1"
                    var toField = typeof(TResult).GetProperty(o.Key);

                    // original value "o.Field1"
                    Expression xOriginal = null;

                    if (!o.Value.StartsWith('['))
                    {
                        // property "Field1"
                        var fromField = typeof(T).GetProperty(o.Value);

                        xOriginal = Expression.Property(xParameter, fromField);
                    }
                    else
                    {
                        xOriginal = Expression.Constant(o.Value.Trim('[').Trim(']'));
                    }

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(toField, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<T, TResult>>(xInit, xParameter);

            // compile to Func<Data, Data>
            //return lambda.Compile();

            return query.Select(lambda);
        }
    }
}
