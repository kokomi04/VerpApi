using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBRestrictionContext: OrganizationDBContext
    {
        private readonly ICurrentContextService _currentContext;

        public OrganizationDBRestrictionContext(DbContextOptions<OrganizationDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<OrganizationDBContext>(loggerFactory))
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
            return await base.SaveChangesAsync(true, cancellationToken);
        }
    }


}
