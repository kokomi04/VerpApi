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
        public virtual DbSet<PurchasingSuggest> PurchasingSuggest { get; set; }
        public virtual DbSet<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
        public virtual DbSet<PurchasingSuggestFile> PurchasingSuggestFile { get; set; }
        public virtual DbSet<Test> Test { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchasingRequest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PurchasingRequestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingRequestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingSuggest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PurchasingSuggestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingSuggestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PrimaryUnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PurchasingRequestCode).HasMaxLength(255);

                entity.Property(e => e.Tax).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingSuggestFile>(entity =>
            {
                entity.HasKey(e => new { e.PurchasingSuggestId, e.FileId });
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.Property(e => e.TestName).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
