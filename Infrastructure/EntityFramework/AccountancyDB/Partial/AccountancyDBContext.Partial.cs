using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class AccountancyDBRestrictionContext : AccountancyDBContext, ICurrentRequestDbContext
    {
        public ICurrentContextService CurrentContextService { get; private set; }

        public AccountancyDBRestrictionContext(DbContextOptions<AccountancyDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<AccountancyDBContext>(loggerFactory))
        {
            CurrentContextService = currentContext;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilterBase();
        }

        public override int SaveChanges()
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(CurrentContextService);
            return await base.SaveChangesAsync();
        }
    }
}
