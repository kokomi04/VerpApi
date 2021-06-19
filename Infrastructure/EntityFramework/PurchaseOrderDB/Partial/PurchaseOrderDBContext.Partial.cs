using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDBContext
    {
        protected DbSet<MaterialCalcProductOrderGroup> _materialCalcProductOrderGroup { get; set; }

        public virtual IQueryable<MaterialCalcProductOrderGroup> MaterialCalcProductOrderGroup
        {
            get
            {               
                var sql = $"SELECT MaterialCalcProductId, STRING_AGG(OrderCode,',') OrderCodes, SUM(OrderProductQuantity) TotalOrderProductQuantity FROM dbo.MaterialCalcProductOrder GROUP BY MaterialCalcProductId";

                return _materialCalcProductOrderGroup.FromSqlRaw(sql);
            }
        }
    }

    public class MaterialCalcProductOrderGroup
    {
        [Key]
        public long MaterialCalcProductId { get; set; }
        public string OrderCodes { get; set; }
        public decimal? TotalOrderProductQuantity { get; set; }

    }
    public class PurchaseOrderDBRestrictionContext : PurchaseOrderDBContext, ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }
        public PurchaseOrderDBRestrictionContext(DbContextOptions<PurchaseOrderDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<PurchaseOrderDBContext>(loggerFactory))
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
}
