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

        public virtual DbSet<InOutStepLink> InOutStepLink { get; set; }
        public virtual DbSet<ProductInStep> ProductInStep { get; set; }
        public virtual DbSet<ProductInStepLink> ProductInStepLink { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual DbSet<ProductionStep> ProductionStep { get; set; }
        public virtual DbSet<ProductionStepLink> ProductionStepLink { get; set; }
        public virtual DbSet<RequestOutsourcePart> RequestOutsourcePart { get; set; }
        public virtual DbSet<Step> Step { get; set; }
        public virtual DbSet<StepGroup> StepGroup { get; set; }
        public virtual DbSet<TrackOutsource> TrackOutsource { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InOutStepLink>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.ProductInStepId })
                    .HasName("PK_InOutStepMapping");

                entity.HasOne(d => d.ProductInStep)
                    .WithMany(p => p.InOutStepLink)
                    .HasForeignKey(d => d.ProductInStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InOutStepMapping_ProductInStep");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.InOutStepLink)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InOutStepMapping_ProductionStep");
            });

            modelBuilder.Entity<ProductInStep>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductInStepLink>(entity =>
            {
                entity.HasKey(e => new { e.InputProductInStepId, e.OutputProductInStepId })
                    .HasName("PK_ProductInStepMapping");
            });

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

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderDetail)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderDetail_ProductionOrder");
            });

            modelBuilder.Entity<ProductionStep>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Step)
                    .WithMany(p => p.ProductionStep)
                    .HasForeignKey(d => d.StepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStep_Step");
            });

            modelBuilder.Entity<ProductionStepLink>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.FromStepId, e.ToStepId })
                    .HasName("PK_ProductionStagesMapping");
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
