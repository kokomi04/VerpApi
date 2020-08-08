using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public static class EfBatchExtensions
    {
        public static async Task BatchInsert<T>(this DbContext dbContext, IList<T> entities, bool outputIdentity = true) where T : class
        {
            var bulkConfig = new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = outputIdentity };
            await dbContext.BulkInsertAsync(entities, bulkConfig);
        }
    }
}
