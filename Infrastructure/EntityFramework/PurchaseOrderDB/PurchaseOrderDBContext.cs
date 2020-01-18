using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDBContext : DbContext
    {
        public PurchaseOrderDBContext()
        {
        }

        public PurchaseOrderDBContext(DbContextOptions<PurchaseOrderDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Test> Test { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Test>(entity =>
            {
                entity.Property(e => e.TestName).HasMaxLength(50);
            });
        }
    }
}
