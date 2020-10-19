using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class EfBatchExtensions
    {
        public static async Task InsertByBatch<T>(this DbContext dbContext, IList<T> entities, bool outputIdentity = true) where T : class
        {
            var bulkConfig = new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = outputIdentity };
            await dbContext.BulkInsertAsync(entities, bulkConfig);
        }

        public static async Task<int> UpdateByBatch<T>(this IQueryable<T> query, Expression<Func<T, T>> updateExpression, CancellationToken cancellationToken = default) where T : class
        {
            return await query.BatchUpdateAsync(updateExpression, cancellationToken);
        }
    }
}
