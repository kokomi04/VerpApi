using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDBContext : ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }
        public PurchaseOrderDBContext(DbContextOptions<PurchaseOrderDBContext> options, ICurrentContextService currentContext)
            : base(options)
        {
            CurrentContextService = currentContext;
            SubsidiaryId = currentContext.SubsidiaryId;
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFilterAuthorize(this);
        }

        public override int SaveChanges()
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return await base.SaveChangesAsync(true, cancellationToken);
        }
    }
}
