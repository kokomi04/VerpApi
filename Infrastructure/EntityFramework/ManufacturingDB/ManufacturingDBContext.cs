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

        public virtual DbSet<OutsourceOrder> OutsourceOrder { get; set; }
        public virtual DbSet<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }
        public virtual DbSet<ProductSemi> ProductSemi { get; set; }
        public virtual DbSet<ProductionAssignment> ProductionAssignment { get; set; }
        public virtual DbSet<ProductionHandover> ProductionHandover { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual DbSet<ProductionSchedule> ProductionSchedule { get; set; }
        public virtual DbSet<ProductionStep> ProductionStep { get; set; }
        public virtual DbSet<ProductionStepInOutConverter> ProductionStepInOutConverter { get; set; }
        public virtual DbSet<ProductionStepLinkData> ProductionStepLinkData { get; set; }
        public virtual DbSet<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual DbSet<ProductionStepOrder> ProductionStepOrder { get; set; }
        public virtual DbSet<ProductionStepRoleClient> ProductionStepRoleClient { get; set; }
        public virtual DbSet<RequestOutsourcePart> RequestOutsourcePart { get; set; }
        public virtual DbSet<RequestOutsourcePartDetail> RequestOutsourcePartDetail { get; set; }
        public virtual DbSet<RequestOutsourceStep> RequestOutsourceStep { get; set; }
        public virtual DbSet<RequestOutsourceStepDetail> RequestOutsourceStepDetail { get; set; }
        public virtual DbSet<Step> Step { get; set; }
        public virtual DbSet<StepGroup> StepGroup { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutsourceOrder>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.FreightCost).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OtherCost).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OutsourceOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.OutsourceOrderFinishDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OutsourceTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.ProviderAddress).HasMaxLength(256);

                entity.Property(e => e.ProviderName).HasMaxLength(128);

                entity.Property(e => e.ProviderPhone).HasMaxLength(20);

                entity.Property(e => e.ProviderReceiver).HasMaxLength(128);

                entity.Property(e => e.TransportToAddress).HasMaxLength(256);

                entity.Property(e => e.TransportToCompany).HasMaxLength(128);

                entity.Property(e => e.TransportToPhone).HasMaxLength(20);

                entity.Property(e => e.TransportToReceiver).HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<OutsourceOrderDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Tax).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceOrderDetail)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceOrderDetail_OutsourceOrder");
            });

            modelBuilder.Entity<ProductSemi>(entity =>
            {
                entity.Property(e => e.ContainerTypeId)
                    .HasDefaultValueSql("((1))")
                    .HasComment("1-SP 2-LSX");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductionAssignment>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.DepartmentId, e.ScheduleTurnId })
                    .HasName("PK_ProductionStepOrder_copy1");

                entity.Property(e => e.AssignmentQuantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionAssignment)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionAssignment_ProductionStep");
            });

            modelBuilder.Entity<ProductionHandover>(entity =>
            {
                entity.Property(e => e.ProductionHandoverId).ValueGeneratedNever();

                entity.Property(e => e.HandoverQuantity).HasColumnType("decimal(18, 5)");
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

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ReserveQuantity)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("Bù hao (dự trữ)");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderDetail)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderDetail_ProductionOrder");
            });

            modelBuilder.Entity<ProductionSchedule>(entity =>
            {
                entity.Property(e => e.ProductionScheduleQuantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionOrderDetail)
                    .WithMany(p => p.ProductionSchedule)
                    .HasForeignKey(d => d.ProductionOrderDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionSchedule_ProductionOrderDetail");
            });

            modelBuilder.Entity<ProductionStep>(entity =>
            {
                entity.Property(e => e.ContainerId).HasComment("ID của Product hoặc lệnh SX");

                entity.Property(e => e.ContainerTypeId).HasComment(@"1: Sản phẩm
2: Lệnh SX");

                entity.Property(e => e.CoordinateX).HasDefaultValueSql("((0))");

                entity.Property(e => e.CoordinateY).HasDefaultValueSql("((0))");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.ParentCode).HasMaxLength(50);

                entity.Property(e => e.ProductionStepCode).HasMaxLength(50);

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

                entity.Property(e => e.ObjectTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.ProductionStepLinkDataCode).HasMaxLength(50);

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

                entity.HasOne(d => d.ProductionOrderDetail)
                    .WithMany(p => p.ProductionStepOrder)
                    .HasForeignKey(d => d.ProductionOrderDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepOrder_ProductionOrderDetail");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionStepOrder)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepOrder_ProductionStep");
            });

            modelBuilder.Entity<ProductionStepRoleClient>(entity =>
            {
                entity.HasKey(e => new { e.ContainerId, e.ContainerTypeId })
                    .HasName("PK_StepClientData");

                entity.Property(e => e.ContainerTypeId).HasComment("1-SP 2-LSX");
            });

            modelBuilder.Entity<RequestOutsourcePart>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.RequestOutsourcePartCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionOrderDetail)
                    .WithMany(p => p.RequestOutsourcePart)
                    .HasForeignKey(d => d.ProductionOrderDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourcePart_ProductionOrderDetail");
            });

            modelBuilder.Entity<RequestOutsourcePartDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.RequestOutsourcePart)
                    .WithMany(p => p.RequestOutsourcePartDetail)
                    .HasForeignKey(d => d.RequestOutsourcePartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourcePartDetail_RequestOutsourcePart");
            });

            modelBuilder.Entity<RequestOutsourceStep>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DateRequiredComplete).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.PathStepId).IsRequired();

                entity.Property(e => e.RequestOutsourceStepCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<RequestOutsourceStepDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionStepLinkData)
                    .WithMany(p => p.RequestOutsourceStepDetail)
                    .HasForeignKey(d => d.ProductionStepLinkDataId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourceStepDetail_ProductionStepLinkData");

                entity.HasOne(d => d.RequestOutsourceStep)
                    .WithMany(p => p.RequestOutsourceStepDetail)
                    .HasForeignKey(d => d.RequestOutsourceStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestOutsourceStepDetail_RequestOutsourceStep");
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
