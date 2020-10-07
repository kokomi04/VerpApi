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

        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputBill> InputBill { get; set; }
        public virtual DbSet<InputField> InputField { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputTypeGroup> InputTypeGroup { get; set; }
        public virtual DbSet<InputTypeView> InputTypeView { get; set; }
        public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }
        public virtual DbSet<PoAssignment> PoAssignment { get; set; }
        public virtual DbSet<PoAssignmentDetail> PoAssignmentDetail { get; set; }
        public virtual DbSet<ProviderProductInfo> ProviderProductInfo { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public virtual DbSet<PurchaseOrderFile> PurchaseOrderFile { get; set; }
        public virtual DbSet<PurchasingRequest> PurchasingRequest { get; set; }
        public virtual DbSet<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
        public virtual DbSet<PurchasingSuggest> PurchasingSuggest { get; set; }
        public virtual DbSet<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
        public virtual DbSet<PurchasingSuggestFile> PurchasingSuggestFile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InputArea>(entity =>
            {
                entity.Property(e => e.Columns).HasDefaultValueSql("((1))");

                entity.Property(e => e.InputAreaCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputArea)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputArea_InputType");
            });

            modelBuilder.Entity<InputAreaField>(entity =>
            {
                entity.HasIndex(e => new { e.InputTypeId, e.InputFieldId })
                    .HasName("IX_InputAreaField")
                    .IsUnique();

                entity.Property(e => e.Column).HasDefaultValueSql("((1))");

                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.Filters).HasMaxLength(512);

                entity.Property(e => e.InputStyleJson).HasMaxLength(512);

                entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.TitleStyleJson).HasMaxLength(512);

                entity.HasOne(d => d.InputArea)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.InputAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_InputArea");

                entity.HasOne(d => d.InputField)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.InputFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_InputField");

                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_InputType");
            });

            modelBuilder.Entity<InputBill>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_InputValueBill");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputBill)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueBill_InputType");
            });

            modelBuilder.Entity<InputField>(entity =>
            {
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RefTableCode).HasMaxLength(128);

                entity.Property(e => e.RefTableField).HasMaxLength(128);

                entity.Property(e => e.RefTableTitle).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);
            });

            modelBuilder.Entity<InputType>(entity =>
            {
                entity.Property(e => e.InputTypeCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.InputTypeGroup)
                    .WithMany(p => p.InputType)
                    .HasForeignKey(d => d.InputTypeGroupId)
                    .HasConstraintName("FK_InputType_InputTypeGroup");
            });

            modelBuilder.Entity<InputTypeGroup>(entity =>
            {
                entity.Property(e => e.InputTypeGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<InputTypeView>(entity =>
            {
                entity.Property(e => e.InputTypeViewName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputTypeView)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputTypeView_InputType");
            });

            modelBuilder.Entity<InputTypeViewField>(entity =>
            {
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RefTableCode).HasMaxLength(128);

                entity.Property(e => e.RefTableField).HasMaxLength(128);

                entity.Property(e => e.RefTableTitle).HasMaxLength(512);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.SelectFilters).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.InputTypeView)
                    .WithMany(p => p.InputTypeViewField)
                    .HasForeignKey(d => d.InputTypeViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputTypeViewField_InputTypeView");
            });

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

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

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
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryUnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode).HasMaxLength(128);

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

            modelBuilder.Entity<PurchaseOrderFile>(entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderId, e.FileId });

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderFile)
                    .HasForeignKey(d => d.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrderFile_PurchaseOrder");
            });

            modelBuilder.Entity<PurchasingRequest>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Date).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.PurchasingRequestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingRequestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode).HasMaxLength(128);

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

                entity.Property(e => e.Date).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.PurchasingSuggestCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PurchasingSuggestDetail>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.OrderCode).HasMaxLength(128);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryUnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode).HasMaxLength(128);

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
