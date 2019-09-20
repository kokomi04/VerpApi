using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockDBContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreated(modelBuilder);

            modelBuilder.Entity<Product>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ProductExtraInfo>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ProductStockInfo>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ProductCate>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ProductIdentityCode>().HasQueryFilter(o => !o.IsDeleted);

        }
    }
    
}
