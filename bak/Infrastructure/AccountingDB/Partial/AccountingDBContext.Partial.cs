using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class AccountingDBRestrictionContext : AccountingDBContext
    {
        private readonly ICurrentContextService _currentContext;

        public AccountingDBRestrictionContext(DbContextOptions<AccountingDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<AccountingDBContext>(loggerFactory))
        {
            _currentContext = currentContext;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilterBase();
        }

        public override int SaveChanges()
        {
            this.SetHistoryBaseValue(_currentContext);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetHistoryBaseValue(_currentContext);
            return await base.SaveChangesAsync();
        }
    }
}
