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

        public virtual DbSet<DepartmentTimeTable> DepartmentTimeTable { get; set; }
        public virtual DbSet<OutsourceOrder> OutsourceOrder { get; set; }
        public virtual DbSet<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }
        public virtual DbSet<OutsourcePartRequest> OutsourcePartRequest { get; set; }
        public virtual DbSet<OutsourcePartRequestDetail> OutsourcePartRequestDetail { get; set; }
        public virtual DbSet<OutsourceStepRequest> OutsourceStepRequest { get; set; }
        public virtual DbSet<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }
        public virtual DbSet<OutsourceTrack> OutsourceTrack { get; set; }
        public virtual DbSet<ProductSemi> ProductSemi { get; set; }
        public virtual DbSet<ProductionAssignment> ProductionAssignment { get; set; }
        public virtual DbSet<ProductionAssignmentDetail> ProductionAssignmentDetail { get; set; }
        public virtual DbSet<ProductionConsumMaterial> ProductionConsumMaterial { get; set; }
        public virtual DbSet<ProductionConsumMaterialDetail> ProductionConsumMaterialDetail { get; set; }
        public virtual DbSet<ProductionHandover> ProductionHandover { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual DbSet<ProductionSchedule> ProductionSchedule { get; set; }
        public virtual DbSet<ProductionScheduleTurnShift> ProductionScheduleTurnShift { get; set; }
        public virtual DbSet<ProductionScheduleTurnShiftUser> ProductionScheduleTurnShiftUser { get; set; }
        public virtual DbSet<ProductionStep> ProductionStep { get; set; }
        public virtual DbSet<ProductionStepInOutConverter> ProductionStepInOutConverter { get; set; }
        public virtual DbSet<ProductionStepLinkData> ProductionStepLinkData { get; set; }
        public virtual DbSet<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual DbSet<ProductionStepOrder> ProductionStepOrder { get; set; }
        public virtual DbSet<ProductionStepRoleClient> ProductionStepRoleClient { get; set; }
        public virtual DbSet<ProductionStepWorkInfo> ProductionStepWorkInfo { get; set; }
        public virtual DbSet<Step> Step { get; set; }
        public virtual DbSet<StepDetail> StepDetail { get; set; }
        public virtual DbSet<StepGroup> StepGroup { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentTimeTable>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.WorkDate })
                    .HasName("PK_ProductionAssignment_copy2_copy1");

                entity.Property(e => e.HourPerDay).HasColumnType("decimal(18, 5)");
            });

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

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Tax).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceOrderDetail)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceOrderDetail_OutsourceOrder");
            });

            modelBuilder.Entity<OutsourcePartRequest>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.OutsourcePartRequestCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionOrderDetail)
                    .WithMany(p => p.OutsourcePartRequest)
                    .HasForeignKey(d => d.ProductionOrderDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourcePartRequest_ProductionOrderDetail");
            });

            modelBuilder.Entity<OutsourcePartRequestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.PathProductIdInBom)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.OutsourcePartRequest)
                    .WithMany(p => p.OutsourcePartRequestDetail)
                    .HasForeignKey(d => d.OutsourcePartRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourcePartRequestDetail_OutsourcePartRequest");
            });

            modelBuilder.Entity<OutsourceStepRequest>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.OutsourceStepRequestCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProductionStepId).HasComment("Lấy ra tên QTSX được gắn với YCGC");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.OutsourceStepRequest)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequest_ProductionOrder");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.OutsourceStepRequest)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequest_ProductionStep");
            });

            modelBuilder.Entity<OutsourceStepRequestData>(entity =>
            {
                entity.HasKey(e => new { e.OutsourceStepRequestId, e.ProductionStepLinkDataId })
                    .HasName("PK_RequestOutsourceStepOutput");

                entity.Property(e => e.ProductionStepLinkDataRoleTypeId).HasComment("1: input 2:input");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.OutsourceStepRequest)
                    .WithMany(p => p.OutsourceStepRequestData)
                    .HasForeignKey(d => d.OutsourceStepRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequestData_OutsourceStepRequest");

                entity.HasOne(d => d.ProductionStepLinkData)
                    .WithMany(p => p.OutsourceStepRequestData)
                    .HasForeignKey(d => d.ProductionStepLinkDataId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequestData_ProductionStepLinkData");
            });

            modelBuilder.Entity<OutsourceTrack>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceTrack)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceTrack_OutsourceOrder");
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
                entity.HasKey(e => new { e.ProductionStepId, e.ScheduleTurnId, e.DepartmentId });

                entity.Property(e => e.AssignmentQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Productivity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionAssignment)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionAssignment_ProductionStep");
            });

            modelBuilder.Entity<ProductionAssignmentDetail>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.ScheduleTurnId, e.DepartmentId, e.WorkDate })
                    .HasName("PK_ProductionAssignment_copy2");

                entity.Property(e => e.QuantityPerDay).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionAssignmentDetail)
                    .HasForeignKey(d => new { d.ProductionStepId, d.ScheduleTurnId, d.DepartmentId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionAssignmentDetail_ProductionAssignment");
            });

            modelBuilder.Entity<ProductionConsumMaterial>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionConsumMaterial)
                    .HasForeignKey(d => new { d.ProductionStepId, d.ScheduleTurnId, d.DepartmentId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionConsumMaterial_ProductionAssignment");
            });

            modelBuilder.Entity<ProductionConsumMaterialDetail>(entity =>
            {
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionConsumMaterial)
                    .WithMany(p => p.ProductionConsumMaterialDetail)
                    .HasForeignKey(d => d.ProductionConsumMaterialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionConsumMaterialDetail_ProductionConsumMaterial");
            });

            modelBuilder.Entity<ProductionHandover>(entity =>
            {
                entity.Property(e => e.HandoverQuantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionHandoverProductionAssignment)
                    .HasForeignKey(d => new { d.FromProductionStepId, d.ScheduleTurnId, d.FromDepartmentId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionHandover_ProductionAssignmentFrom");

                entity.HasOne(d => d.ProductionAssignmentNavigation)
                    .WithMany(p => p.ProductionHandoverProductionAssignmentNavigation)
                    .HasForeignKey(d => new { d.ToProductionStepId, d.ScheduleTurnId, d.ToDepartmentId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionHandover_ProductionAssignmentTo");
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

            modelBuilder.Entity<ProductionScheduleTurnShift>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Hours).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionScheduleTurnShift)
                    .HasForeignKey(d => new { d.ProductionStepId, d.ScheduleTurnId, d.DepartmentId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionScheduleTurnShift_ProductionAssignment");
            });

            modelBuilder.Entity<ProductionScheduleTurnShiftUser>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Money).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionScheduleTurnShift)
                    .WithMany(p => p.ProductionScheduleTurnShiftUser)
                    .HasForeignKey(d => d.ProductionScheduleTurnShiftId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionScheduleTurnShiftUser_ProductionScheduleTurnShift");
            });

            modelBuilder.Entity<ProductionStep>(entity =>
            {
                entity.Property(e => e.ContainerId).HasComment("ID của Product hoặc lệnh SX");

                entity.Property(e => e.ContainerTypeId).HasComment(@"1: Sản phẩm
2: Lệnh SX");

                entity.Property(e => e.CoordinateX)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CoordinateY)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.ParentCode).HasMaxLength(50);

                entity.Property(e => e.ProductionStepCode).HasMaxLength(50);

                entity.Property(e => e.StepId).HasComment("NULL nếu là quy trình con");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Workload)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("khoi luong cong viec");

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

                entity.Property(e => e.ExportOutsourceQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ObjectTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.OutsourceQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ProductionStepLinkDataCode).HasMaxLength(50);

                entity.Property(e => e.ProductionStepLinkDataTypeId).HasComment("1-GC chi tiet, 2-GC cong doan, 0-default");

                entity.Property(e => e.ProductionStepLinkTypeId).HasDefaultValueSql("((1))");

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

            modelBuilder.Entity<ProductionStepWorkInfo>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.ScheduleTurnId })
                    .HasName("PK_ProductionAssignment_copy1");

                entity.Property(e => e.MaxHour).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.MinHour).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionStepWorkInfo)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionStepWorkInfo_ProductionStep");
            });

            modelBuilder.Entity<Step>(entity =>
            {
                entity.Property(e => e.StepName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UnitId).HasComment("Đơn vị tính năng xuất (/h)");

                entity.HasOne(d => d.StepGroup)
                    .WithMany(p => p.Step)
                    .HasForeignKey(d => d.StepGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Step_StepGroup");
            });

            modelBuilder.Entity<StepDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Step)
                    .WithMany(p => p.StepDetail)
                    .HasForeignKey(d => d.StepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StepDetail_Step");
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
