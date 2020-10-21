using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class UnAuthorizeMasterDBContext : MasterDBContext
    {
        public UnAuthorizeMasterDBContext(DbContextOptions<UnAuthorizeMasterDBContext> options
           , ILoggerFactory loggerFactory)
           : base(options.ChangeOptionsType<MasterDBContext>(loggerFactory))
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilterBase();
        }
    }

    public class MasterDBRestrictionContext : MasterDBContext, ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }
        public MasterDBRestrictionContext(DbContextOptions<MasterDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<MasterDBContext>(loggerFactory))
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
