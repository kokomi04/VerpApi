using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.StockDB
{
    public class StockDBSubsidiaryContext : StockDBContext, ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }

        public ICurrentContextService CurrentContextService { get; private set; }

        public StockDBSubsidiaryContext(DbContextOptions<StockDBSubsidiaryContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<StockDBContext>(loggerFactory))
        {
            CurrentContextService = currentContext;

            SubsidiaryId = currentContext.SubsidiaryId;

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddFilterAuthorize(this);

        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override int SaveChanges()
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public class StockDBRestrictionContext : StockDBContext, IDbContextFilterTypeCache, ISubsidiayRequestDbContext, IStockRequestDbContext
    {
        public List<int> StockIds { get; set; }
        public int SubsidiaryId { get; private set; }

        public bool IgnoreFilterSubsidiary { get; private set; }
        public bool IgnoreFilterStock { get; private set; }

        public ICurrentContextService CurrentContextService { get; private set; }

        public StockDBRestrictionContext(DbContextOptions<StockDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<StockDBContext>(loggerFactory))
        {
            CurrentContextService = currentContext;

            StockIds = currentContext.StockIds?.ToList();
            SubsidiaryId = currentContext.SubsidiaryId;

            IgnoreFilterSubsidiary = false;
            IgnoreFilterStock = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddFilterAuthorize(this);

        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override int SaveChanges()
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }

}
