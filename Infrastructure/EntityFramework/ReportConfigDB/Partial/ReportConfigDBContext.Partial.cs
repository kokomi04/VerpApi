using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public class ReportConfigDBRestrictionContext : ReportConfigDBContext, ISubsidiayRequestDbContext
    {
        public ICurrentContextService CurrentContextService { get; private set; }

        public int SubsidiaryId { get; private set; }

        public ReportConfigDBRestrictionContext(DbContextOptions<ReportConfigDBRestrictionContext> options
            , ICurrentContextService _currentContextService
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<ReportConfigDBContext>(loggerFactory))
        {
            CurrentContextService = _currentContextService;
            SubsidiaryId = _currentContextService.SubsidiaryId;
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
