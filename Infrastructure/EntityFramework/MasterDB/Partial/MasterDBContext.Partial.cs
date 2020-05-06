using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class MasterDBContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFilterBase();
        }
    }
}
