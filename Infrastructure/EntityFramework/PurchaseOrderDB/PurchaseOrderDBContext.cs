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

        public virtual DbSet<PoAssignment> PoAssignment { get; set; }
        public virtual DbSet<PoAssignmentDetail> PoAssignmentDetail { get; set; }
        public virtual DbSet<ProviderProductInfo> ProviderProductInfo { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public virtual DbSet<PurchasingRequest> PurchasingRequest { get; set; }
        public virtual DbSet<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
        public virtual DbSet<PurchasingSuggest> PurchasingSuggest { get; set; }
        public virtual DbSet<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
        public virtual DbSet<PurchasingSuggestFile> PurchasingSuggestFile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PoAssignment>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.PoAssignmentCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.PurchasingSuggest)
                    .WithMany(p => p.PoAssignment)
                    .HasForeignKey(d => d.PurchasingSuggestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PoAssignment_PurchasingSuggest");
            });

            modelBuilder.Entity<PoAssignmentDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryUnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TaxInMoney).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TaxInPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PoAssignment)
                    .WithMany(p => p.PoAssignmentDetail)
                    .HasForeignKey(d => d.PoAssignmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PoAssignmentDetail_PoAssignment");

                entity.HasOne(d => d.PurchasingSuggestDetail)
                    .WithMany(p => p.PoAssignmentDetail)
                    .HasForeignKey(d => d.PurchasingSuggestDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PoAssignmentDetail_PurchasingSuggestDetail");
            });

            modelBuilder.Entity<ProviderProductInfo>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.CustomerId });

                entity.Property(e => e.ProviderProductName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.Property(e => e.AdditionNote).HasMaxLength(512);

                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.DeliveryDestination).HasMaxLength(1024);

                entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.OtherFee).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PaymentInfo).HasMaxLength(512);

                entity.Property(e => e.PurchaseOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TotalMoney).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.PoAssignment)
                    .WithMany(p => p.PurchaseOrder)
                    .HasForeignKey(d => d.PoAssignmentId)
                    .HasConstraintName("FK_PurchaseOrder_PoAssignment1");
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryUnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProviderProductName).HasMaxLength(128);

                entity.Property(e => e.TaxInMoney).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TaxInPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PoAssignmentDetail)
                    .WithMany(p => p.PurchaseOrderDetail)
                    .HasForeignKey(d => d.PoAssignmentDetailId)
                    .HasConstraintName("FK_PurchaseOrderDetail_PoAssignmentDetail");

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderDetail)
                    .HasForeignKey(d => d.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrderDetail_PurchaseOrder");

                entity.HasOne(d => d.PurchasingSuggestDetail)
                    .WithMany(p => p.PurchaseOrderDetail)
                    .HasForeignKey(d => d.PurchasingSuggestDetailId)
                    .HasConstraintName("FK_PurchaseOrderDetail_PurchasingSuggestDetail");
            });

            modelBuilder.Entity<PurchasingRequest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PurchasingRequestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingRequestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PurchasingRequest)
                    .WithMany(p => p.PurchasingRequestDetail)
                    .HasForeignKey(d => d.PurchasingRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchasingRequestDetail_PurchasingRequest");
            });

            modelBuilder.Entity<PurchasingSuggest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

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

                entity.Property(e => e.PurchasingRequestIds).HasMaxLength(256);

                entity.Property(e => e.TaxInMoney).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TaxInPercent).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.PurchasingRequestDetail)
                    .WithMany(p => p.PurchasingSuggestDetail)
                    .HasForeignKey(d => d.PurchasingRequestDetailId)
                    .HasConstraintName("FK_PurchasingSuggestDetail_PurchasingRequestDetail");

                entity.HasOne(d => d.PurchasingSuggest)
                    .WithMany(p => p.PurchasingSuggestDetail)
                    .HasForeignKey(d => d.PurchasingSuggestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchasingSuggestDetail_PurchasingSuggest");
            });

            modelBuilder.Entity<PurchasingSuggestFile>(entity =>
            {
                entity.HasKey(e => new { e.PurchasingSuggestId, e.FileId });

                entity.HasOne(d => d.PurchasingSuggest)
                    .WithMany(p => p.PurchasingSuggestFile)
                    .HasForeignKey(d => d.PurchasingSuggestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchasingSuggestFile_PurchasingSuggest");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
