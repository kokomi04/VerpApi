using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.StockDB
{
    public static class StockDBContextExtensions
    {
        public static IQueryable<Stock> AllStockBySub(this StockDBContext stockDBContext, ICurrentContextService currentContextService)
        {
            return stockDBContext.Stock.IgnoreQueryFilters().Where(s => !s.IsDeleted && s.SubsidiaryId == currentContextService.SubsidiaryId);
        }

    }


    public class StockDBRestrictionContext : StockDBContext, IDbContextFilterTypeCache, ICurrentRequestDbContext
    {
        //ICurrentContextService _currentContext;
        public List<int> StockIds { get; set; }
        public int SubsidiaryId { get; private set; }

        public bool FilterStock { get; private set; }
        public bool FilterSubsidiary { get; private set; }

        public ICurrentContextService CurrentContextService { get; private set; }

        public StockDBRestrictionContext(DbContextOptions<StockDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<StockDBContext>(loggerFactory))
        {
            // _currentContext = currentContext;
            CurrentContextService = currentContext;

            StockIds = currentContext.StockIds?.ToList();
            SubsidiaryId = currentContext.SubsidiaryId;

            FilterStock = StockIds != null;
            FilterSubsidiary = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ctxConstant = Expression.Constant(this);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty(GlobalFieldConstants.IsDeleted);
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter(GlobalFieldConstants.IsDeleted, isDeleted);
                }

                if (FilterStock)
                {
                    var isStockIdProp = entityType.FindProperty(GlobalFieldConstants.StockId);
                    if (isStockIdProp != null)
                    {
                        var stockIds = Expression.PropertyOrField(ctxConstant, nameof(StockIds));
                        filterBuilder.AddFilterListContains<int>(GlobalFieldConstants.StockId, stockIds);
                    }
                }

                if (FilterSubsidiary)
                {
                    var isSubsidiaryIdProp = entityType.FindProperty(GlobalFieldConstants.SubsidiaryId);
                    if (isSubsidiaryIdProp != null)
                    {
                        var subsidiaryId = Expression.PropertyOrField(ctxConstant, nameof(SubsidiaryId));
                        filterBuilder.AddFilter(GlobalFieldConstants.SubsidiaryId, subsidiaryId);
                    }
                }

                entityType.SetQueryFilter(filterBuilder.Build());
            }

        }
    }

}
