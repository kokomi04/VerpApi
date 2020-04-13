using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.StockDB
{
    public class StockDBRestrictionContext : StockDBContext, IDbContextFilterTypeCache
    {
        //ICurrentContextService _currentContext;
        public List<int> StockIds { get; set; }

        public bool FilterStock { get; private set; }

        public StockDBRestrictionContext(DbContextOptions<StockDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<StockDBContext>(loggerFactory))
        {
            // _currentContext = currentContext;
            StockIds = currentContext.StockIds?.ToList();
            FilterStock = StockIds != null;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ctxConstant = Expression.Constant(this);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter("IsDeleted", isDeleted);
                }

                if (FilterStock)
                {
                    var isStockIdProp = entityType.FindProperty("StockId");
                    if (isStockIdProp != null)
                    {
                        var stockIds = Expression.PropertyOrField(ctxConstant, "StockIds");
                        filterBuilder.AddFilterListContains<int>("StockId", stockIds);
                    }
                }

                entityType.SetQueryFilter(filterBuilder.Build());
            }

        }
    }

}
