﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace ActivityLogDB
{
    public partial class ActivityLogDBRestrictionContext : ActivityLogDBContext, ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }
        public ActivityLogDBRestrictionContext(DbContextOptions<ActivityLogDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<ActivityLogDBContext>(loggerFactory))
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
