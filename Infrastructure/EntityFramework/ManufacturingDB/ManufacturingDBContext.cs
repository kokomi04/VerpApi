using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ManufacturingDBContext : DbContext
{
    public ManufacturingDBContext(DbContextOptions<ManufacturingDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DraftData> DraftData { get; set; }

    public virtual DbSet<IgnoreAllocation> IgnoreAllocation { get; set; }

    //public virtual DbSet<MaterialAllocation> MaterialAllocation { get; set; }

    public virtual DbSet<MonthPlan> MonthPlan { get; set; }

    public virtual DbSet<OutsourcePartRequest> OutsourcePartRequest { get; set; }

    public virtual DbSet<OutsourcePartRequestDetail> OutsourcePartRequestDetail { get; set; }

    public virtual DbSet<OutsourceStepRequest> OutsourceStepRequest { get; set; }

    public virtual DbSet<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }

    public virtual DbSet<ProductSemi> ProductSemi { get; set; }

    public virtual DbSet<ProductSemiConversion> ProductSemiConversion { get; set; }

    public virtual DbSet<ProductionAssignment> ProductionAssignment { get; set; }

    public virtual DbSet<ProductionAssignmentDetail> ProductionAssignmentDetail { get; set; }

    public virtual DbSet<ProductionConsumMaterial> ProductionConsumMaterial { get; set; }

    public virtual DbSet<ProductionConsumMaterialDetail> ProductionConsumMaterialDetail { get; set; }

    public virtual DbSet<ProductionContainer> ProductionContainer { get; set; }

    public virtual DbSet<ProductionHandover> ProductionHandover { get; set; }

    public virtual DbSet<ProductionHandoverReceipt> ProductionHandoverReceipt { get; set; }

    public virtual DbSet<ProductionHistory> ProductionHistory { get; set; }

    public virtual DbSet<ProductionHumanResource> ProductionHumanResource { get; set; }

    public virtual DbSet<ProductionMaterialsRequirement> ProductionMaterialsRequirement { get; set; }

    public virtual DbSet<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; }

    public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }

    public virtual DbSet<ProductionOrderAttachment> ProductionOrderAttachment { get; set; }

    public virtual DbSet<ProductionOrderConfiguration> ProductionOrderConfiguration { get; set; }

    public virtual DbSet<ProductionOrderDetail> ProductionOrderDetail { get; set; }

    public virtual DbSet<ProductionOrderInventoryConflict> ProductionOrderInventoryConflict { get; set; }

    public virtual DbSet<ProductionOrderMaterialSet> ProductionOrderMaterialSet { get; set; }

    public virtual DbSet<ProductionOrderMaterialSetConsumptionGroup> ProductionOrderMaterialSetConsumptionGroup { get; set; }

    public virtual DbSet<ProductionOrderMaterials> ProductionOrderMaterials { get; set; }

    public virtual DbSet<ProductionOrderMaterialsConsumption> ProductionOrderMaterialsConsumption { get; set; }

    public virtual DbSet<ProductionOrderStatus> ProductionOrderStatus { get; set; }

    public virtual DbSet<ProductionOutsourcePartMapping> ProductionOutsourcePartMapping { get; set; }

    public virtual DbSet<ProductionPlanExtraInfo> ProductionPlanExtraInfo { get; set; }

    public virtual DbSet<ProductionProcessMold> ProductionProcessMold { get; set; }

    public virtual DbSet<ProductionStep> ProductionStep { get; set; }

    public virtual DbSet<ProductionStepCollection> ProductionStepCollection { get; set; }

    public virtual DbSet<ProductionStepLinkData> ProductionStepLinkData { get; set; }

    public virtual DbSet<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }

    public virtual DbSet<ProductionStepMold> ProductionStepMold { get; set; }

    public virtual DbSet<ProductionStepMoldLink> ProductionStepMoldLink { get; set; }

    public virtual DbSet<ProductionStepRoleClient> ProductionStepRoleClient { get; set; }

    public virtual DbSet<ProductionStepWorkInfo> ProductionStepWorkInfo { get; set; }

    public virtual DbSet<ProductionWeekPlan> ProductionWeekPlan { get; set; }

    public virtual DbSet<ProductionWeekPlanDetail> ProductionWeekPlanDetail { get; set; }

    public virtual DbSet<RefCustomer> RefCustomer { get; set; }

    public virtual DbSet<RefInventory> RefInventory { get; set; }

    public virtual DbSet<RefOutsourcePartOrder> RefOutsourcePartOrder { get; set; }

    public virtual DbSet<RefOutsourcePartTrack> RefOutsourcePartTrack { get; set; }

    public virtual DbSet<RefOutsourceStepOrder> RefOutsourceStepOrder { get; set; }

    public virtual DbSet<RefOutsourceStepTrack> RefOutsourceStepTrack { get; set; }

    public virtual DbSet<RefProduct> RefProduct { get; set; }

    public virtual DbSet<RefPropertyCalc> RefPropertyCalc { get; set; }

    public virtual DbSet<Step> Step { get; set; }

    public virtual DbSet<StepDetail> StepDetail { get; set; }

    public virtual DbSet<StepGroup> StepGroup { get; set; }

    public virtual DbSet<TargetProductivity> TargetProductivity { get; set; }

    public virtual DbSet<TargetProductivityDetail> TargetProductivityDetail { get; set; }

    public virtual DbSet<WeekPlan> WeekPlan { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DraftData>(entity =>
        {
            entity.HasKey(e => new { e.ObjectTypeId, e.SubsidiaryId, e.ObjectId });
        });

        modelBuilder.Entity<IgnoreAllocation>(entity =>
        {
            entity.HasKey(e => new { e.ProductionOrderId, e.InventoryCode, e.ProductId });

            entity.Property(e => e.InventoryCode).HasMaxLength(128);
        });

        //modelBuilder.Entity<MaterialAllocation>(entity =>
        //{
        //    entity.Property(e => e.AllocationQuantity).HasColumnType("decimal(32, 12)");
        //    entity.Property(e => e.InventoryCode)
        //        .IsRequired()
        //        .HasMaxLength(128);
        //    entity.Property(e => e.ProductId).HasComment("Product id in inventory detail");
        //    entity.Property(e => e.SourceProductId).HasComment("Product id in production process");
        //    entity.Property(e => e.SourceQuantity)
        //        .HasComment("Product quantity output in production process")
        //        .HasColumnType("decimal(32, 12)");
        //});

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

        modelBuilder.Entity<OutsourcePartRequest>(entity =>
        {
            entity.HasKey(e => e.OutsourcePartRequestId).HasName("PK_OutsourceComposition");

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

            entity.HasOne(d => d.ProductionOrderDetail).WithMany(p => p.OutsourcePartRequest)
                .HasForeignKey(d => d.ProductionOrderDetailId)
                .HasConstraintName("FK_OutsourcePartRequest_ProductionOrderDetail");
        });

        modelBuilder.Entity<OutsourcePartRequestDetail>(entity =>
        {
            entity.HasKey(e => e.OutsourcePartRequestDetailId).HasName("PK_RequestOutsourcePartId");

            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.PathProductIdInBom)
                .HasMaxLength(50)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

            entity.HasOne(d => d.OutsourcePartRequest).WithMany(p => p.OutsourcePartRequestDetail)
                .HasForeignKey(d => d.OutsourcePartRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutsourcePartRequestDetail_OutsourcePartRequest");
        });

        modelBuilder.Entity<OutsourceStepRequest>(entity =>
        {
            entity.HasKey(e => e.OutsourceStepRequestId).HasName("PK_RequestOutsourceStep");

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

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.OutsourceStepRequest)
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
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.OutsourceStepRequest).WithMany(p => p.OutsourceStepRequestData)
                .HasForeignKey(d => d.OutsourceStepRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutsourceStepRequestData_OutsourceStepRequest");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.OutsourceStepRequestData)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutsourceStepRequestData_ProductionStep");

            entity.HasOne(d => d.ProductionStepLinkData).WithMany(p => p.OutsourceStepRequestData)
                .HasForeignKey(d => d.ProductionStepLinkDataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutsourceStepRequestData_ProductionStepLinkData");
        });

        modelBuilder.Entity<ProductSemi>(entity =>
        {
            entity.Property(e => e.ContainerTypeId)
                .HasDefaultValueSql("((1))")
                .HasComment("1-SP 2-LSX");
            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(512);
            entity.Property(e => e.RefProductId).HasComment("value of information copied from");
            entity.Property(e => e.Specification).HasMaxLength(128);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProductSemiConversion>(entity =>
        {
            entity.Property(e => e.ConversionRate).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.ProductSemi).WithMany(p => p.ProductSemiConversion)
                .HasForeignKey(d => d.ProductSemiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductSemiConversion_ProductSemi");
        });

        modelBuilder.Entity<ProductionAssignment>(entity =>
        {
            entity.HasKey(e => new { e.ProductionStepId, e.DepartmentId, e.ProductionOrderId });

            entity.Property(e => e.ProductionStepId).HasDefaultValueSql("((1))");
            entity.Property(e => e.AssignmentHours).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.AssignmentQuantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.AssignmentWorkload).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.Comment).HasMaxLength(512);
            entity.Property(e => e.RateInPercent)
                .HasDefaultValueSql("((100))")
                .HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.ProductionStepLinkData).WithMany(p => p.ProductionAssignment)
                .HasForeignKey(d => d.ProductionStepLinkDataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductionAssignment_ProductionStep");
        });

        modelBuilder.Entity<ProductionAssignmentDetail>(entity =>
        {
            entity.HasKey(e => new { e.ProductionStepId, e.DepartmentId, e.WorkDate, e.ProductionOrderId }).HasName("PK_ProductionAssignment_copy2");

            entity.Property(e => e.HoursPerDay).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.MinAssignHours).HasColumnType("decimal(32, 21)");
            entity.Property(e => e.QuantityPerDay).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.WorkloadPerDay).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionAssignment).WithMany(p => p.ProductionAssignmentDetail)
                .HasForeignKey(d => new { d.ProductionStepId, d.DepartmentId, d.ProductionOrderId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionAssignmentDetail_ProductionAssignment");
        });

        modelBuilder.Entity<ProductionConsumMaterial>(entity =>
        {
            entity.HasKey(e => e.ProductionConsumMaterialId).HasName("PK_ProductionScheduleTurnShift_copy1");

            entity.ToTable(tb => tb.HasComment("Khai báo vật tư tiêu hao"));

            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.ProductionConsumMaterial)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionConsumMaterial_ProductionStep");

            entity.HasOne(d => d.ProductionAssignment).WithMany(p => p.ProductionConsumMaterial)
                .HasForeignKey(d => new { d.ProductionStepId, d.DepartmentId, d.ProductionOrderId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionConsumMaterial_ProductionAssignment");
        });

        modelBuilder.Entity<ProductionConsumMaterialDetail>(entity =>
        {
            entity.HasKey(e => e.ProductionConsumMaterialDetailId).HasName("PK__Producti__1EBDA9B817ED695A_copy1");

            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionConsumMaterial).WithMany(p => p.ProductionConsumMaterialDetail)
                .HasForeignKey(d => d.ProductionConsumMaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionConsumMaterialDetail_ProductionConsumMaterial");
        });

        modelBuilder.Entity<ProductionContainer>(entity =>
        {
            entity.HasKey(e => new { e.ContainerTypeId, e.ContainerId });

            entity.Property(e => e.ContainerId).HasComment("ID của Product hoặc lệnh SX");
            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProductionHandover>(entity =>
        {
            entity.HasKey(e => e.ProductionHandoverId).HasName("PK__Producti__1EBDA9B817ED695A");

            entity.HasIndex(e => new { e.ProductionOrderId, e.FromDepartmentId, e.ObjectId, e.ObjectTypeId, e.Status, e.FromProductionStepId, e.ToDepartmentId, e.ToProductionStepId, e.HandoverDatetime }, "IDX_ProductionHandover_Search");

            entity.Property(e => e.HandoverQuantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.InventoryCode).HasMaxLength(128);
            entity.Property(e => e.InventoryProductId).HasComment("Product id in production process");
            entity.Property(e => e.InventoryQuantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.FromProductionStep).WithMany(p => p.ProductionHandoverFromProductionStep)
                .HasForeignKey(d => d.FromProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHandover_FromProductionStep");

            entity.HasOne(d => d.ProductionHandoverReceipt).WithMany(p => p.ProductionHandover)
                .HasForeignKey(d => d.ProductionHandoverReceiptId)
                .HasConstraintName("FK_ProductionHandover_ProductionHandoverReceipt");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionHandover)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHandover_ProductionOrder");

            entity.HasOne(d => d.ToProductionStep).WithMany(p => p.ProductionHandoverToProductionStep)
                .HasForeignKey(d => d.ToProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHandover_ToProductionStep");
        });

        modelBuilder.Entity<ProductionHandoverReceipt>(entity =>
        {
            entity.HasIndex(e => new { e.SubsidiaryId, e.ProductionHandoverReceiptCode }, "IX_ProductionHandoverReceipt_ProductionHandoverReceiptCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.ProductionHandoverReceiptCode).HasMaxLength(128);
        });

        modelBuilder.Entity<ProductionHistory>(entity =>
        {
            entity.HasKey(e => e.ProductionHistoryId).HasName("PK__ProductionHistory");

            entity.Property(e => e.OvertimeProductionQuantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.ProductionQuantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionHandoverReceipt).WithMany(p => p.ProductionHistory)
                .HasForeignKey(d => d.ProductionHandoverReceiptId)
                .HasConstraintName("FK_ProductionHistory_ProductionHandoverReceipt");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.ProductionHistory)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHistory_ProductionStep");
        });

        modelBuilder.Entity<ProductionHumanResource>(entity =>
        {
            entity.Property(e => e.OfficeWorkDay).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.OvertimeWorkDay).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionHumanResource)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHumanResource_ProductionOrder");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.ProductionHumanResource)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionHumanResource_ProductionStep");
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

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionMaterialsRequirement)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionMaterialsRequirement_ProductionOrder");
        });

        modelBuilder.Entity<ProductionMaterialsRequirementDetail>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.OutsourceStepRequest).WithMany(p => p.ProductionMaterialsRequirementDetail)
                .HasForeignKey(d => d.OutsourceStepRequestId)
                .HasConstraintName("FK_ProductionMaterialsRequirementDetail_OutsourceStepRequest");

            entity.HasOne(d => d.ProductionMaterialsRequirement).WithMany(p => p.ProductionMaterialsRequirementDetail)
                .HasForeignKey(d => d.ProductionMaterialsRequirementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionMaterialsRequirementDetail_ProductionMaterialsRequirement");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.ProductionMaterialsRequirementDetail)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionMaterialsRequirementDetail_ProductionStep");
        });

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.ProductionOrderId).HasName("PK__ProductionOrder");

            entity.HasIndex(e => new { e.SubsidiaryId, e.ProductionOrderCode }, "IX_ProductionOrder_ProductionOrderCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(128);
            entity.Property(e => e.EndDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ProductionOrderCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ProductionOrderProcessVersion)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.ProductionOrderStatus).HasDefaultValueSql("((1))");
            entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FromWeekPlan).WithMany(p => p.ProductionOrderFromWeekPlan)
                .HasForeignKey(d => d.FromWeekPlanId)
                .HasConstraintName("FK_ProductionOrder_FromWeekPlan");

            entity.HasOne(d => d.MonthPlan).WithMany(p => p.ProductionOrder)
                .HasForeignKey(d => d.MonthPlanId)
                .HasConstraintName("FK_ProductionOrder_MonthPlan");

            entity.HasOne(d => d.ToWeekPlan).WithMany(p => p.ProductionOrderToWeekPlan)
                .HasForeignKey(d => d.ToWeekPlanId)
                .HasConstraintName("FK_ProductionOrder_ToWeekPlan");
        });

        modelBuilder.Entity<ProductionOrderAttachment>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(256);

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionOrderAttachment)
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
            entity.HasKey(e => e.ProductionOrderDetailId).HasName("PK__Producti__6DBD23C6D219D575");

            entity.Property(e => e.Note).HasMaxLength(128);
            entity.Property(e => e.OrderCode).HasMaxLength(50);
            entity.Property(e => e.PartnerId).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.ReserveQuantity)
                .HasComment("Bù hao (dự trữ)")
                .HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionOrderDetail)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionOrderDetail_ProductionOrder");
        });

        modelBuilder.Entity<ProductionOrderInventoryConflict>(entity =>
        {
            entity.HasKey(e => new { e.ProductionOrderId, e.InventoryDetailId });

            entity.Property(e => e.Content).HasMaxLength(512);
            entity.Property(e => e.HandoverInventoryQuantitySum).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.InventoryCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.InventoryQuantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.InventoryRequirementCode).HasMaxLength(128);
            entity.Property(e => e.RequireQuantity).HasColumnType("decimal(32, 12)");
        });

        modelBuilder.Entity<ProductionOrderMaterialSet>(entity =>
        {
            entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(128);
            entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionOrderMaterialSet)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionOrderMaterialSet_ProductionOrder");
        });

        modelBuilder.Entity<ProductionOrderMaterialSetConsumptionGroup>(entity =>
        {
            entity.HasKey(e => new { e.ProductionOrderMaterialSetId, e.ProductMaterialsConsumptionGroupId });

            entity.HasOne(d => d.ProductionOrderMaterialSet).WithMany(p => p.ProductionOrderMaterialSetConsumptionGroup)
                .HasForeignKey(d => d.ProductionOrderMaterialSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionOrderMaterialSetConsumptionGroup_ProductionOrderMaterialSet");
        });

        modelBuilder.Entity<ProductionOrderMaterials>(entity =>
        {
            entity.Property(e => e.ConversionRate)
                .HasDefaultValueSql("((1))")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.IdClient).HasMaxLength(128);
            entity.Property(e => e.InventoryRequirementStatusId).HasDefaultValueSql("((1))");
            entity.Property(e => e.ParentIdClient).HasMaxLength(128);
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_ProductionOrderMaterials_ProductionOrderMaterials");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionOrderMaterials)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionOrderMaterials_ProductionOrder");

            entity.HasOne(d => d.ProductionOrderMaterialSet).WithMany(p => p.ProductionOrderMaterials)
                .HasForeignKey(d => d.ProductionOrderMaterialSetId)
                .HasConstraintName("FK_ProductionOrderMaterials_ProductionOrderMaterialSet");

            entity.HasOne(d => d.ProductionStepLinkData).WithMany(p => p.ProductionOrderMaterials)
                .HasForeignKey(d => d.ProductionStepLinkDataId)
                .HasConstraintName("FK_ProductionOrderMaterials_ProductionStepLinkData");
        });

        modelBuilder.Entity<ProductionOrderMaterialsConsumption>(entity =>
        {
            entity.Property(e => e.ConversionRate).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_ProductionOrderMaterialsConsumption_ProductionOrderMaterialsConsumption");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionOrderMaterialsConsumption)
                .HasForeignKey(d => d.ProductionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionOrderMaterialsConsumption_ProductionOrder");
        });

        modelBuilder.Entity<ProductionOrderStatus>(entity =>
        {
            entity.Property(e => e.ProductionOrderStatusId).ValueGeneratedNever();
            entity.Property(e => e.CssStyle).HasMaxLength(1024);
            entity.Property(e => e.Description).HasMaxLength(128);
            entity.Property(e => e.ProductionOrderStatusName).HasMaxLength(128);
        });

        modelBuilder.Entity<ProductionOutsourcePartMapping>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");
        });

        modelBuilder.Entity<ProductionPlanExtraInfo>(entity =>
        {
            entity.HasKey(e => new { e.MonthPlanId, e.SubsidiaryId, e.ProductionOrderDetailId });

            entity.HasOne(d => d.ProductionOrderDetail).WithMany(p => p.ProductionPlanExtraInfo)
                .HasForeignKey(d => d.ProductionOrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionPlanExtraInfo_ProductionOrderDetail");
        });

        modelBuilder.Entity<ProductionProcessMold>(entity =>
        {
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<ProductionStep>(entity =>
        {
            entity.HasKey(e => e.ProductionStepId).HasName("PK_ProductionStages");

            entity.Property(e => e.Comment).HasMaxLength(512);
            entity.Property(e => e.ContainerId).HasComment("ID của Product hoặc lệnh SX");
            entity.Property(e => e.ContainerTypeId).HasComment("1: Sản phẩm\r\n2: Lệnh SX");
            entity.Property(e => e.CoordinateX)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CoordinateY)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.OutsourceStepRequestId).HasComment("");
            entity.Property(e => e.ParentCode).HasMaxLength(50);
            entity.Property(e => e.ProductionStepCode).HasMaxLength(50);
            entity.Property(e => e.StepId).HasComment("NULL nếu là quy trình con");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.Workload)
                .HasComment("khoi luong cong viec")
                .HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.OutsourceStepRequest).WithMany(p => p.ProductionStep)
                .HasForeignKey(d => d.OutsourceStepRequestId)
                .HasConstraintName("FK_ProductionStep_OutsourceStepRequest");

            entity.HasOne(d => d.Step).WithMany(p => p.ProductionStep)
                .HasForeignKey(d => d.StepId)
                .HasConstraintName("FK_ProductionStep_Step");
        });

        modelBuilder.Entity<ProductionStepCollection>(entity =>
        {
            entity.ToTable(tb => tb.HasComment("Quy trình thường gặp"));

            entity.Property(e => e.Collections).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(256);
        });

        modelBuilder.Entity<ProductionStepLinkData>(entity =>
        {
            entity.HasKey(e => e.ProductionStepLinkDataId).HasName("PK_ProductionStagesDetail");

            entity.HasIndex(e => new { e.SubsidiaryId, e.LinkDataObjectId, e.LinkDataObjectTypeId }, "IDX_ProductionStepLinkData_Object");

            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.ExportOutsourceQuantity)
                .HasComment("Số lượng NVL đầu vào của nhóm công đoạn đi gia công công đoạn cần xuất đi gia công")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.ObjectIdBak).HasColumnName("ObjectId_bak");
            entity.Property(e => e.ObjectTypeIdBak)
                .HasDefaultValueSql("((1))")
                .HasColumnName("ObjectTypeId_bak");
            entity.Property(e => e.OutsourcePartQuantity)
                .HasComment("Số lượng gia công chi tiết")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.OutsourceQuantity)
                .HasComment("Số lượng gia công công đoạn")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.ProductionStepLinkDataCode).HasMaxLength(50);
            entity.Property(e => e.ProductionStepLinkDataTypeId).HasComment("1-GC chi tiet, 2-GC cong doan, 0-default");
            entity.Property(e => e.ProductionStepLinkTypeId).HasDefaultValueSql("((1))");
            entity.Property(e => e.Quantity)
                .HasComment("Số lượng sản xuất")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.QuantityOrigin)
                .HasComment("Số lượng gốc (trong thiết kế QTSX ban đầu khi chưa kéo)")
                .HasColumnType("decimal(32, 12)");
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.WorkloadConvertRate).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.OutsourceRequestDetail).WithMany(p => p.ProductionStepLinkData)
                .HasForeignKey(d => d.OutsourceRequestDetailId)
                .HasConstraintName("FK_ProductionStepLinkData_OutsourcePartRequestDetail");
        });

        modelBuilder.Entity<ProductionStepLinkDataRole>(entity =>
        {
            entity.HasKey(e => new { e.ProductionStepLinkDataId, e.ProductionStepId }).HasName("PK_InOutStepMapping");

            entity.Property(e => e.ProductionStepLinkDataGroupBak)
                .HasMaxLength(50)
                .HasColumnName("ProductionStepLinkDataGroup_bak");
            entity.Property(e => e.ProductionStepLinkDataRoleTypeId).HasComment("1: Input\r\n2: Output");

            entity.HasOne(d => d.ProductionStep).WithMany(p => p.ProductionStepLinkDataRole)
                .HasForeignKey(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepLinkDataRole_ProductionStep");

            entity.HasOne(d => d.ProductionStepLinkData).WithMany(p => p.ProductionStepLinkDataRole)
                .HasForeignKey(d => d.ProductionStepLinkDataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepLinkDataRole_ProductionStepLinkData");
        });

        modelBuilder.Entity<ProductionStepMold>(entity =>
        {
            entity.ToTable(tb => tb.HasComment("Quy trình mẫu"));

            entity.Property(e => e.CoordinateX).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CoordinateY).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ProductionProcessMold).WithMany(p => p.ProductionStepMold)
                .HasForeignKey(d => d.ProductionProcessMoldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepMold_ProductionProcessMold");

            entity.HasOne(d => d.Step).WithMany(p => p.ProductionStepMold)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepMold_Step");
        });

        modelBuilder.Entity<ProductionStepMoldLink>(entity =>
        {
            entity.HasIndex(e => new { e.FromProductionStepMoldId, e.ToProductionStepMoldId }, "IX_ProductionStepMoldLink").IsUnique();

            entity.HasOne(d => d.FromProductionStepMold).WithMany(p => p.ProductionStepMoldLinkFromProductionStepMold)
                .HasForeignKey(d => d.FromProductionStepMoldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepMoldRole_ProductionStepMold_From");

            entity.HasOne(d => d.ToProductionStepMold).WithMany(p => p.ProductionStepMoldLinkToProductionStepMold)
                .HasForeignKey(d => d.ToProductionStepMoldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionStepMoldLink_ProductionStepMold_To");
        });

        modelBuilder.Entity<ProductionStepRoleClient>(entity =>
        {
            entity.HasKey(e => new { e.ContainerId, e.ContainerTypeId }).HasName("PK_StepClientData");

            entity.Property(e => e.ContainerTypeId).HasComment("1-SP 2-LSX");
        });

        modelBuilder.Entity<ProductionStepWorkInfo>(entity =>
        {
            entity.HasKey(e => e.ProductionStepId).HasName("PK_ProductionAssignment_copy1");

            entity.Property(e => e.ProductionStepId).ValueGeneratedNever();
            entity.Property(e => e.MaxHour).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.MinHour).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.ProductionStep).WithOne(p => p.ProductionStepWorkInfo)
                .HasForeignKey<ProductionStepWorkInfo>(d => d.ProductionStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductionStepWorkInfo_ProductionStep");
        });

        modelBuilder.Entity<ProductionWeekPlan>(entity =>
        {
            entity.HasKey(e => e.ProductionWeekPlanId).HasName("PK__Producti__A75C11E74636BA55");

            entity.Property(e => e.ProductQuantity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.ProductionOrderDetail).WithMany(p => p.ProductionWeekPlan)
                .HasForeignKey(d => d.ProductionOrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionWeekPlan_ProductionOrderDetail");
        });

        modelBuilder.Entity<ProductionWeekPlanDetail>(entity =>
        {
            entity.HasKey(e => new { e.ProductionWeekPlanId, e.ProductCateId }).HasName("PK__Producti__E1D52AC5B60EB4AC");

            entity.Property(e => e.MaterialQuantity).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.ProductionWeekPlan).WithMany(p => p.ProductionWeekPlanDetail)
                .HasForeignKey(d => d.ProductionWeekPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductionWeekPlanDetail_ProductionWeekPlan");
        });

        modelBuilder.Entity<RefCustomer>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefCustomer");

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

        modelBuilder.Entity<RefInventory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefInventory");

            entity.Property(e => e.InventoryCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.InventoryId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<RefOutsourcePartOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefOutsourcePartOrder");

            entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(38, 12)");
            entity.Property(e => e.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<RefOutsourcePartTrack>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefOutsourcePartTrack");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");
        });

        modelBuilder.Entity<RefOutsourceStepOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefOutsourceStepOrder");

            entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(38, 12)");
            entity.Property(e => e.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<RefOutsourceStepTrack>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefOutsourceStepTrack");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Quantity).HasColumnType("decimal(32, 12)");
        });

        modelBuilder.Entity<RefProduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefProduct");

            entity.Property(e => e.Barcode).HasMaxLength(128);
            entity.Property(e => e.EstimatePrice).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Height).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.LoadAbility).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Long).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Measurement).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.PackingMethod).HasMaxLength(255);
            entity.Property(e => e.ProductCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ProductInternalName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ProductNameEng).HasMaxLength(255);
            entity.Property(e => e.Quantitative).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Width).HasColumnType("decimal(18, 5)");
        });

        modelBuilder.Entity<RefPropertyCalc>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefPropertyCalc");

            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.PropertyCalcCode).HasMaxLength(128);
            entity.Property(e => e.PropertyCalcId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.StepId).HasName("PK__Step__24343357236BB9F6");

            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.HandoverTypeId).HasDefaultValueSql("((1))");
            entity.Property(e => e.ShrinkageRate).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.StepName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.StepGroup).WithMany(p => p.Step)
                .HasForeignKey(d => d.StepGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Step_StepGroup");
        });

        modelBuilder.Entity<StepDetail>(entity =>
        {
            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.QuantityBak)
                .HasComment("Nang suat/nguoi-may")
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("Quantity_bak");
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

            entity.HasOne(d => d.Step).WithMany(p => p.StepDetail)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StepDetail_Step");
        });

        modelBuilder.Entity<StepGroup>(entity =>
        {
            entity.HasKey(e => e.StepGroupId).HasName("PK__StepGrou__16B2127FE65CFA15");

            entity.Property(e => e.StepGroupName)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<TargetProductivity>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.EstimateProductionDays).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.EstimateProductionQuantity).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Note).HasMaxLength(1024);
            entity.Property(e => e.TargetProductivityCode)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<TargetProductivityDetail>(entity =>
        {
            entity.Property(e => e.MinAssignHours).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.ProductivityResourceTypeId).HasDefaultValueSql("((1))");
            entity.Property(e => e.ProductivityTimeTypeId).HasDefaultValueSql("((2))");
            entity.Property(e => e.TargetProductivity).HasColumnType("decimal(32, 12)");
            entity.Property(e => e.WorkLoadTypeId)
                .HasDefaultValueSql("((1))")
                .HasComment("Option tính KLCV tính năng suất theo KL Tinh hay theo số lượng");

            entity.HasOne(d => d.TargetProductivityNavigation).WithMany(p => p.TargetProductivityDetail)
                .HasForeignKey(d => d.TargetProductivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TargetProductivityDetail_TargetProductivityDetail");
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
