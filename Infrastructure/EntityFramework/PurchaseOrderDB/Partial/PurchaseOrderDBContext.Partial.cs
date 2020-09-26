using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDBContext : ICurrentRequestDbContext
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
            var ctxConstant = Expression.Constant(this);

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
                if (isSubsidiaryIdProp != null)
                {
                    var subsidiaryId = Expression.PropertyOrField(ctxConstant, nameof(SubsidiaryId));
                    filterBuilder.AddFilter(GlobalFieldConstants.SubsidiaryId, subsidiaryId);
                }


                entityType.SetQueryFilter(filterBuilder.Build());
            }
        }
    }
}
