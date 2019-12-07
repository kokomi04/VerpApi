using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockDBContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreated(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter("IsDeleted", isDeleted);
                }

                entityType.QueryFilter = filterBuilder.Build();
            }

        }
    }
    
}
