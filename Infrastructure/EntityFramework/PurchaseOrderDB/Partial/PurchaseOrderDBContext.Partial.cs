using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDBContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFilterBase();
        }
    }
}
