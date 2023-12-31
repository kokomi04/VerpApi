﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public class AccountancyDBPrivateContext : AccountancyDBContext, ISubsidiayRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }
        public AccountancyDBPrivateContext(DbContextOptions<AccountancyDBPrivateContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<AccountancyDBContext>(loggerFactory))
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

    public sealed class AccountancyDBPublicContext : AccountancyDBPrivateContext
    {
        public AccountancyDBPublicContext(DbContextOptions<AccountancyDBPublicContext> options, ICurrentContextService currentContext, ILoggerFactory loggerFactory) : 
            base(options.ChangeOptionsType<AccountancyDBPrivateContext>(loggerFactory), currentContext, loggerFactory)
        {
        }
    }
  

    public sealed class UnAuthorizeAccountancyDBPublicContext : AccountancyDBContext
    {
        public UnAuthorizeAccountancyDBPublicContext(DbContextOptions<UnAuthorizeAccountancyDBPublicContext> options
           , ILoggerFactory loggerFactory)
           : base(options.ChangeOptionsType<AccountancyDBContext>(loggerFactory))
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilterBase();
        }
    }
}
