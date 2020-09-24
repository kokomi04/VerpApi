using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBRestrictionContext : OrganizationDBContext, ICurrentRequestDbContext
    {
        public int SubsidiaryId { get; private set; }
        public ICurrentContextService CurrentContextService { get; private set; }

        public OrganizationDBRestrictionContext(DbContextOptions<OrganizationDBRestrictionContext> options
            , ICurrentContextService currentContext
            , ILoggerFactory loggerFactory)
            : base(options.ChangeOptionsType<OrganizationDBContext>(loggerFactory))
        {
            CurrentContextService = currentContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddFilterBase();

            var ctxConstant = Expression.Constant(this);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {

                var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

                var isDeletedProp = entityType.FindProperty(GlobalFieldConstants.IsDeleted);
                if (isDeletedProp != null)
                {
                    var isDeleted = Expression.Constant(false);
                    filterBuilder.AddFilter(GlobalFieldConstants.IsDeleted, isDeleted);
                }


                var isSubsidiaryIdProp = entityType.FindProperty(GlobalFieldConstants.SubsidiaryId);
                if (isSubsidiaryIdProp != null && entityType.ClrType.Name != nameof(Subsidiary) && entityType.ClrType.Name != nameof(EmployeeSubsidiary))
                {
                    var subsidiaryId = Expression.PropertyOrField(ctxConstant, nameof(SubsidiaryId));
                    filterBuilder.AddFilter(GlobalFieldConstants.SubsidiaryId, subsidiaryId);
                }


                entityType.SetQueryFilter(filterBuilder.Build());
            }

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
