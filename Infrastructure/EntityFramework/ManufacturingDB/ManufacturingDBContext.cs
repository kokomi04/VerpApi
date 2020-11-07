using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ManufacturingDBContext : DbContext
    {
        public ManufacturingDBContext()
        {
        }

        public ManufacturingDBContext(DbContextOptions<ManufacturingDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual DbSet<ProductionStep> ProductionStep { get; set; }
        public virtual DbSet<ProductionStepInOutConverter> ProductionStepInOutConverter { get; set; }
        public virtual DbSet<ProductionStepLinkData> ProductionStepLinkData { get; set; }
        public virtual DbSet<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual DbSet<ProductionStepOrder> ProductionStepOrder { get; set; }
        public virtual DbSet<RequestOutsourcePart> RequestOutsourcePart { get; set; }
        public virtual DbSet<Step> Step { get; set; }
        public virtual DbSet<StepGroup> StepGroup { get; set; }
        public virtual DbSet<TrackOutsource> TrackOutsource { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductionOrder>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.ProductionOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ProductionOrderDetail>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(128);

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderDetail)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderDetail_ProductionOrder");
            });

            modelBuilder.Entity<ProductionStep>(entity =>
            {
                entity.Property(e => e.ContainerId).HasComment("ID của Product hoặc lệnh SX");

                entity.Property(e => e.ContainerTypeId).HasComment(@"1: Sản phẩm
2: Lệnh SX");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.StepId).HasComment("NULL nếu là quy trình con");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Step)
                    .WithMany(p => p.ProductionStep)
                    .HasForeignKey(d => d.StepId)
                    .HasConstraintName("FK_ProductionStep_Step");
            });

            modelBuilder.Entity<ProductionStepInOutConverter>(entity =>
            {
                entity.HasKey(e => new { e.InputProductionStepLinkDataId, e.OutputProductionStepLinkDataId });
            });

            modelBuilder.Entity<ProductionStepLinkData>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductionStepLinkDataRole>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepLinkDataId, e.ProductionStepId })
                    .HasName("PK_InOutStepMapping");

                entity.Property(e => e.ProductionStepLinkDataRoleTypeId).HasComment(@"1: Input
2: Output");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionStepLinkDataRole)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepLinkDataRole_ProductionStep");

                entity.HasOne(d => d.ProductionStepLinkData)
                    .WithMany(p => p.ProductionStepLinkDataRole)
                    .HasForeignKey(d => d.ProductionStepLinkDataId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepLinkDataRole_ProductionStepLinkData");
            });

            modelBuilder.Entity<ProductionStepOrder>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.ProductionOrderDetailId });
            });

            modelBuilder.Entity<RequestOutsourcePart>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.RequestOrder)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductInStep)
                    .WithMany(p => p.RequestOutsourcePart)
                    .HasForeignKey(d => d.ProductInStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourcePart_ProductInStep");

                entity.HasOne(d => d.ProductionOrderDetail)
                    .WithMany(p => p.RequestOutsourcePart)
                    .HasForeignKey(d => d.ProductionOrderDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourcePart_ProductionOrderDetail");
            });

            modelBuilder.Entity<Step>(entity =>
            {
                entity.Property(e => e.StepName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.StepGroup)
                    .WithMany(p => p.Step)
                    .HasForeignKey(d => d.StepGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Step_StepGroup");
            });

            modelBuilder.Entity<StepGroup>(entity =>
            {
                entity.Property(e => e.StepGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<TrackOutsource>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DateTrack).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
