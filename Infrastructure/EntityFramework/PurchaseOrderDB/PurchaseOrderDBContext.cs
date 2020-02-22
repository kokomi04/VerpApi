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

        public virtual DbSet<PurchasingRequest> PurchasingRequest { get; set; }
        public virtual DbSet<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
        public virtual DbSet<Test> Test { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<PurchasingRequest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetime).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PurchasingRequestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetime).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingRequestDetail>(entity =>
            {
                entity.Property(e => e.PurchasingRequestDetailId).ValueGeneratedNever();

                entity.Property(e => e.CreatedDatetime).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetime).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.Property(e => e.TestName).HasMaxLength(50);
            });
        }
    }
}
