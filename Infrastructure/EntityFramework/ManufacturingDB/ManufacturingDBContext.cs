using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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

        public virtual DbSet<DraftData> DraftData { get; set; }
        public virtual DbSet<MonthPlan> MonthPlan { get; set; }
        public virtual DbSet<OutsourceOrder> OutsourceOrder { get; set; }
        public virtual DbSet<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }
        public virtual DbSet<OutsourceOrderExcess> OutsourceOrderExcess { get; set; }
        public virtual DbSet<OutsourceOrderMaterials> OutsourceOrderMaterials { get; set; }
        public virtual DbSet<OutsourcePartRequest> OutsourcePartRequest { get; set; }
        public virtual DbSet<OutsourcePartRequestDetail> OutsourcePartRequestDetail { get; set; }
        public virtual DbSet<OutsourceStepRequest> OutsourceStepRequest { get; set; }
        public virtual DbSet<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }
        public virtual DbSet<OutsourceTrack> OutsourceTrack { get; set; }
        public virtual DbSet<ProductSemi> ProductSemi { get; set; }
        public virtual DbSet<ProductSemiConversion> ProductSemiConversion { get; set; }
        public virtual DbSet<ProductionAssignment> ProductionAssignment { get; set; }
        public virtual DbSet<ProductionAssignmentDetail> ProductionAssignmentDetail { get; set; }
        public virtual DbSet<ProductionConsumMaterial> ProductionConsumMaterial { get; set; }
        public virtual DbSet<ProductionConsumMaterialDetail> ProductionConsumMaterialDetail { get; set; }
        public virtual DbSet<ProductionHandover> ProductionHandover { get; set; }
        public virtual DbSet<ProductionMaterialsRequirement> ProductionMaterialsRequirement { get; set; }
        public virtual DbSet<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<ProductionOrderAttachment> ProductionOrderAttachment { get; set; }
        public virtual DbSet<ProductionOrderConfiguration> ProductionOrderConfiguration { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual DbSet<ProductionOrderMaterials> ProductionOrderMaterials { get; set; }
        public virtual DbSet<ProductionOrderMaterialsConsumption> ProductionOrderMaterialsConsumption { get; set; }
        public virtual DbSet<ProductionOrderStatus> ProductionOrderStatus { get; set; }
        public virtual DbSet<ProductionPlanExtraInfo> ProductionPlanExtraInfo { get; set; }
        public virtual DbSet<ProductionProcessMold> ProductionProcessMold { get; set; }
        public virtual DbSet<ProductionScheduleTurnShift> ProductionScheduleTurnShift { get; set; }
        public virtual DbSet<ProductionScheduleTurnShiftUser> ProductionScheduleTurnShiftUser { get; set; }
        public virtual DbSet<ProductionStep> ProductionStep { get; set; }
        public virtual DbSet<ProductionStepCollection> ProductionStepCollection { get; set; }
        public virtual DbSet<ProductionStepInOutConverter> ProductionStepInOutConverter { get; set; }
        public virtual DbSet<ProductionStepLinkData> ProductionStepLinkData { get; set; }
        public virtual DbSet<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual DbSet<ProductionStepMold> ProductionStepMold { get; set; }
        public virtual DbSet<ProductionStepMoldLink> ProductionStepMoldLink { get; set; }
        public virtual DbSet<ProductionStepRoleClient> ProductionStepRoleClient { get; set; }
        public virtual DbSet<ProductionStepWorkInfo> ProductionStepWorkInfo { get; set; }
        public virtual DbSet<ProductionWeekPlan> ProductionWeekPlan { get; set; }
        public virtual DbSet<ProductionWeekPlanDetail> ProductionWeekPlanDetail { get; set; }
        public virtual DbSet<RefCustomer> RefCustomer { get; set; }
        public virtual DbSet<RefOutsourcePartOrder> RefOutsourcePartOrder { get; set; }
        public virtual DbSet<RefOutsourcePartTrack> RefOutsourcePartTrack { get; set; }
        public virtual DbSet<RefOutsourceStepOrder> RefOutsourceStepOrder { get; set; }
        public virtual DbSet<RefOutsourceStepTrack> RefOutsourceStepTrack { get; set; }
        public virtual DbSet<RefProduct> RefProduct { get; set; }
        public virtual DbSet<RefPropertyCalc> RefPropertyCalc { get; set; }
        public virtual DbSet<Step> Step { get; set; }
        public virtual DbSet<StepDetail> StepDetail { get; set; }
        public virtual DbSet<StepGroup> StepGroup { get; set; }
        public virtual DbSet<WeekPlan> WeekPlan { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<DraftData>(entity =>
            {
                entity.HasKey(e => new { e.ObjectTypeId, e.SubsidiaryId, e.ObjectId });
            });

            modelBuilder.Entity<MonthPlan>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.MonthNote).HasMaxLength(512);

                entity.Property(e => e.MonthPlanName)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<OutsourceOrder>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.OutsourceOrderCode }, "IX_OutsourceOrder_OutsourceOrderCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.HasIndex(e => e.PropertyCalcId, "IX_OutsourceOrder_PropertyCalcId")
                    .HasFilter("([IsDeleted]=(0) AND [PropertyCalcId] IS NOT NULL)");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeliveryDestination).HasMaxLength(1024);

                entity.Property(e => e.FreightCost).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OtherCost).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OutsourceOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.OutsourceOrderFinishDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OutsourceTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Suppliers)
                    .HasMaxLength(1024)
                    .HasComment("");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<OutsourceOrderDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.Quantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.Tax).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceOrderDetail)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceOrderDetail_OutsourceOrder");
            });

            modelBuilder.Entity<OutsourceOrderExcess>(entity =>
            {
                entity.Property(e => e.DecimalPlace).HasDefaultValueSql("((12))");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Specification).HasMaxLength(255);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceOrderExcess)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceOrderExcess_OutsourceOrder");
            });

            modelBuilder.Entity<OutsourceOrderMaterials>(entity =>
            {
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.OutsourceOrder)
                    .WithMany(p => p.OutsourceOrderMaterials)
                    .HasForeignKey(d => d.OutsourceOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceOrderMaterials_OutsourceOrder");
            });

            modelBuilder.Entity<OutsourcePartRequest>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.OutsourcePartRequestCode }, "IX_OutsourcePartRequest_OutsourcePartRequestCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.OutsourcePartRequestCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OutsourcePartRequestStatusId)
                    .HasDefaultValueSql("((1))")
                    .HasComment("");

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
                entity.HasIndex(e => new { e.SubsidiaryId, e.OutsourceStepRequestCode }, "IX_OutsourceStepRequest_OutsourceStepRequestCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.OutsourceStepRequestCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OutsourceStepRequestStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.OutsourceStepRequest)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequest_ProductionOrder");
            });

            modelBuilder.Entity<OutsourceStepRequestData>(entity =>
            {
                entity.HasKey(e => new { e.OutsourceStepRequestId, e.ProductionStepId, e.ProductionStepLinkDataId });

                entity.Property(e => e.IsImportant)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ProductionStepLinkDataRoleTypeId).HasComment("1: input 2:input");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.OutsourceStepRequest)
                    .WithMany(p => p.OutsourceStepRequestData)
                    .HasForeignKey(d => d.OutsourceStepRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequestData_OutsourceStepRequest");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.OutsourceStepRequestData)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsourceStepRequestData_ProductionStep");

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

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("");

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

                entity.Property(e => e.Note).HasMaxLength(512);

                entity.Property(e => e.Specification).HasMaxLength(128);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductSemiConversion>(entity =>
            {
                entity.Property(e => e.ConversionRate).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductSemi)
                    .WithMany(p => p.ProductSemiConversion)
                    .HasForeignKey(d => d.ProductSemiId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductSemiConversion_ProductSemi");
            });

            modelBuilder.Entity<ProductionAssignment>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.DepartmentId, e.ProductionOrderId });

                entity.Property(e => e.ProductionStepId).HasDefaultValueSql("((1))");

                entity.Property(e => e.AssignmentQuantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionStepLinkData)
                    .WithMany(p => p.ProductionAssignment)
                    .HasForeignKey(d => d.ProductionStepLinkDataId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionAssignment_ProductionStep");
            });

            modelBuilder.Entity<ProductionAssignmentDetail>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepId, e.DepartmentId, e.WorkDate, e.ProductionOrderId })
                    .HasName("PK_ProductionAssignment_copy2");

                entity.Property(e => e.QuantityPerDay).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionAssignmentDetail)
                    .HasForeignKey(d => new { d.ProductionStepId, d.DepartmentId, d.ProductionOrderId })
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
                    .HasForeignKey(d => new { d.ProductionStepId, d.DepartmentId, d.ProductionOrderId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionConsumMaterial_ProductionAssignment");
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
            });

            modelBuilder.Entity<ProductionMaterialsRequirement>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.RequirementCode }, "IX_ProductionMaterialsRequirement_RequirementCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.RequirementCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.RequirementContent).HasMaxLength(512);

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionMaterialsRequirement)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionMaterialsRequirement_ProductionOrder");
            });

            modelBuilder.Entity<ProductionMaterialsRequirementDetail>(entity =>
            {
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionMaterialsRequirement)
                    .WithMany(p => p.ProductionMaterialsRequirementDetail)
                    .HasForeignKey(d => d.ProductionMaterialsRequirementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionMaterialsRequirementDetail_ProductionMaterialsRequirement");

                entity.HasOne(d => d.ProductionStep)
                    .WithMany(p => p.ProductionMaterialsRequirementDetail)
                    .HasForeignKey(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionMaterialsRequirementDetail_ProductionStep");
            });

            modelBuilder.Entity<ProductionOrder>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.ProductionOrderCode }, "IX_ProductionOrder_ProductionOrderCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.EndDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProductionOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ProductionOrderStatus).HasDefaultValueSql("((1))");

                entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ProductionOrderAttachment>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(256);

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderAttachment)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderAttachment_ProductionOrder");
            });

            modelBuilder.Entity<ProductionOrderConfiguration>(entity =>
            {
                entity.Property(e => e.NumberOfDayPed).HasColumnName("NumberOfDayPED");
            });

            modelBuilder.Entity<ProductionOrderDetail>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(128);

                entity.Property(e => e.OrderCode).HasMaxLength(50);

                entity.Property(e => e.PartnerId).HasMaxLength(50);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ReserveQuantity)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("Bù hao (dự trữ)");

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderDetail)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderDetail_ProductionOrder");
            });

            modelBuilder.Entity<ProductionOrderMaterials>(entity =>
            {
                entity.Property(e => e.ConversionRate)
                    .HasColumnType("decimal(18, 5)")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.InventoryRequirementStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_ProductionOrderMaterials_ProductionOrderMaterials");

                entity.HasOne(d => d.ProductionOrder)
                    .WithMany(p => p.ProductionOrderMaterials)
                    .HasForeignKey(d => d.ProductionOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderMaterials_ProductionOrder");
            });

            modelBuilder.Entity<ProductionOrderMaterialsConsumption>(entity =>
            {
                entity.Property(e => e.ConversionRate).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");
            });

            modelBuilder.Entity<ProductionOrderStatus>(entity =>
            {
                entity.Property(e => e.ProductionOrderStatusId).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.ProductionOrderStatusName).HasMaxLength(128);
            });

            modelBuilder.Entity<ProductionPlanExtraInfo>(entity =>
            {
                entity.HasKey(e => new { e.MonthPlanId, e.SubsidiaryId, e.ProductionOrderDetailId });
            });

            modelBuilder.Entity<ProductionProcessMold>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ProductionScheduleTurnShift>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Hours).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.ProductionAssignment)
                    .WithMany(p => p.ProductionScheduleTurnShift)
                    .HasForeignKey(d => new { d.ProductionStepId, d.DepartmentId, d.ProductionOrderId })
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

                entity.Property(e => e.ContainerTypeId).HasComment("1: Sản phẩm\r\n2: Lệnh SX");

                entity.Property(e => e.CoordinateX)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CoordinateY)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.OutsourceStepRequestId).HasComment("");

                entity.Property(e => e.ParentCode).HasMaxLength(50);

                entity.Property(e => e.ProductionStepCode).HasMaxLength(50);

                entity.Property(e => e.StepId).HasComment("NULL nếu là quy trình con");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Workload)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("khoi luong cong viec");

                entity.HasOne(d => d.OutsourceStepRequest)
                    .WithMany(p => p.ProductionStep)
                    .HasForeignKey(d => d.OutsourceStepRequestId)
                    .HasConstraintName("FK_ProductionStep_OutsourceStepRequest");

                entity.HasOne(d => d.Step)
                    .WithMany(p => p.ProductionStep)
                    .HasForeignKey(d => d.StepId)
                    .HasConstraintName("FK_ProductionStep_Step");
            });

            modelBuilder.Entity<ProductionStepCollection>(entity =>
            {
                entity.Property(e => e.Collections).IsRequired();

                entity.Property(e => e.Title).HasMaxLength(256);
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

                entity.Property(e => e.OutsourcePartQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OutsourceQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ProductionStepLinkDataCode).HasMaxLength(50);

                entity.Property(e => e.ProductionStepLinkDataTypeId).HasComment("1-GC chi tiet, 2-GC cong doan, 0-default");

                entity.Property(e => e.ProductionStepLinkTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.QuantityOrigin).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.WorkloadConvertRate)
                    .HasColumnType("decimal(18, 5)")
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<ProductionStepLinkDataRole>(entity =>
            {
                entity.HasKey(e => new { e.ProductionStepLinkDataId, e.ProductionStepId })
                    .HasName("PK_InOutStepMapping");

                entity.Property(e => e.ProductionStepLinkDataGroup).HasMaxLength(50);

                entity.Property(e => e.ProductionStepLinkDataRoleTypeId).HasComment("1: Input\r\n2: Output");

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

            modelBuilder.Entity<ProductionStepMold>(entity =>
            {
                entity.Property(e => e.CoordinateX).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CoordinateY).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.ProductionProcessMold)
                    .WithMany(p => p.ProductionStepMold)
                    .HasForeignKey(d => d.ProductionProcessMoldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepMold_ProductionProcessMold");

                entity.HasOne(d => d.Step)
                    .WithMany(p => p.ProductionStepMold)
                    .HasForeignKey(d => d.StepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepMold_Step");
            });

            modelBuilder.Entity<ProductionStepMoldLink>(entity =>
            {
                entity.HasKey(e => new { e.FromProductionStepMoldId, e.ToProductionStepMoldId });

                entity.HasOne(d => d.FromProductionStepMold)
                    .WithMany(p => p.ProductionStepMoldLinkFromProductionStepMold)
                    .HasForeignKey(d => d.FromProductionStepMoldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepMoldRole_ProductionStepMold_From");

                entity.HasOne(d => d.ToProductionStepMold)
                    .WithMany(p => p.ProductionStepMoldLinkToProductionStepMold)
                    .HasForeignKey(d => d.ToProductionStepMoldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionStepMoldLink_ProductionStepMold_To");
            });

            modelBuilder.Entity<ProductionStepRoleClient>(entity =>
            {
                entity.HasKey(e => new { e.ContainerId, e.ContainerTypeId })
                    .HasName("PK_StepClientData");

                entity.Property(e => e.ContainerTypeId).HasComment("1-SP 2-LSX");
            });

            modelBuilder.Entity<ProductionStepWorkInfo>(entity =>
            {
                entity.HasKey(e => e.ProductionStepId)
                    .HasName("PK_ProductionAssignment_copy1");

                entity.Property(e => e.ProductionStepId).ValueGeneratedNever();

                entity.Property(e => e.MaxHour).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.MinHour).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.ProductionStep)
                    .WithOne(p => p.ProductionStepWorkInfo)
                    .HasForeignKey<ProductionStepWorkInfo>(d => d.ProductionStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductionStepWorkInfo_ProductionStep");
            });

            modelBuilder.Entity<ProductionWeekPlan>(entity =>
            {
                entity.Property(e => e.ProductQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductionWeekPlanDetail>(entity =>
            {
                entity.HasKey(e => new { e.ProductionWeekPlanId, e.ProductCateId })
                    .HasName("PK__Producti__E1D52AC5B60EB4AC");

                entity.Property(e => e.MaterialQuantity).HasColumnType("decimal(32, 16)");

                entity.HasOne(d => d.ProductionWeekPlan)
                    .WithMany(p => p.ProductionWeekPlanDetail)
                    .HasForeignKey(d => d.ProductionWeekPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionWeekPlanDetail_ProductionWeekPlan");
            });

            modelBuilder.Entity<RefCustomer>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefCustomer");

                entity.Property(e => e.Address).HasMaxLength(128);

                entity.Property(e => e.CustomerCode).HasMaxLength(128);

                entity.Property(e => e.CustomerId).ValueGeneratedOnAdd();

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.DebtLimitation).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.Identify).HasMaxLength(64);

                entity.Property(e => e.LegalRepresentative).HasMaxLength(128);

                entity.Property(e => e.LoanLimitation).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.Property(e => e.Website).HasMaxLength(128);
            });

            modelBuilder.Entity<RefOutsourcePartOrder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefOutsourcePartOrder");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PurchaseOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<RefOutsourcePartTrack>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefOutsourcePartTrack");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.PurchaseOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");
            });

            modelBuilder.Entity<RefOutsourceStepOrder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefOutsourceStepOrder");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PurchaseOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<RefOutsourceStepTrack>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefOutsourceStepTrack");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.PurchaseOrderCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");
            });

            modelBuilder.Entity<RefProduct>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefProduct");

                entity.Property(e => e.Barcode).HasMaxLength(128);

                entity.Property(e => e.EstimatePrice).HasColumnType("decimal(19, 4)");

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LoadAbility).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Long).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Measurement).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PackingMethod).HasMaxLength(255);

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ProductId).ValueGeneratedOnAdd();

                entity.Property(e => e.ProductInternalName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ProductNameEng).HasMaxLength(255);

                entity.Property(e => e.Quantitative).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Width).HasColumnType("decimal(18, 4)");
            });

            modelBuilder.Entity<RefPropertyCalc>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefPropertyCalc");

                entity.Property(e => e.Description).HasMaxLength(1024);

                entity.Property(e => e.PropertyCalcCode).HasMaxLength(128);

                entity.Property(e => e.PropertyCalcId).ValueGeneratedOnAdd();

                entity.Property(e => e.Title).HasMaxLength(128);
            });

            modelBuilder.Entity<Step>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.HandoverTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Productivity)
                    .HasColumnType("decimal(18, 5)")
                    .HasDefaultValueSql("((0))")
                    .HasComment("Nang suat/nguoi-may");

                entity.Property(e => e.ShrinkageRate).HasColumnType("decimal(18, 5)");

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

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18, 5)")
                    .HasComment("Nang suat/nguoi-may");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.WorkingHours).HasColumnType("decimal(18, 5)");

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

            modelBuilder.Entity<WeekPlan>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.WeekNote).HasMaxLength(512);

                entity.Property(e => e.WeekPlanName)
                    .IsRequired()
                    .HasMaxLength(512);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
