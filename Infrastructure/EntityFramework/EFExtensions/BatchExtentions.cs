using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class BatchExtentions
    {
        public static Task<int> UpdateBatch<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, TEntity>> updateFactory) where TEntity : class
        {
            return query
                .IgnoreQueryFilters() // reagrading to https://github.com/zzzprojects/EntityFramework-Plus/issues/417
                .UpdateAsync(updateFactory);
        }

        public static Task<int> DeleteBatch<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            return query
                .IgnoreQueryFilters() // reagrading to https://github.com/zzzprojects/EntityFramework-Plus/issues/417
                .DeleteAsync();
        }

        public static Task InsertBatch<TEntity>(this DbContext context, IList<TEntity> entities) where TEntity : class
        {
            if (entities == null || entities.Count == 0)
            {
                return Task.CompletedTask;
            }          

            var config = new BulkConfig
            {
                BatchSize = 2000,
                TrackingEntities = false,
                SetOutputIdentity = false,
                PreserveInsertOrder = false,
            };
            return context.BulkInsertAsync(entities, config);
        }
    }
}
