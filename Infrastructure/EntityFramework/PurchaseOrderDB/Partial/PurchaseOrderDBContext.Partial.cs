using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MasterDBContext
    {
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    OnModelCreated(modelBuilder);

        //    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        //    {

        //        var filterBuilder = new FilterExpressionBuilder(entityType.ClrType);

        //        var isDeletedProp = entityType.FindProperty("IsDeleted");
        //        if (isDeletedProp != null)
        //        {
        //            var isDeleted = Expression.Constant(false);
        //            filterBuilder.AddFilter("IsDeleted", isDeleted);
        //        }

        //        entityType.QueryFilter = filterBuilder.Build();
        //    }

        //}
    }


}
