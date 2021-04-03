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
        public virtual DbSet<PurchaseOrderFile> PurchaseOrderFile { get; set; }
        public virtual DbSet<PurchasingRequest> PurchasingRequest { get; set; }
        public virtual DbSet<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
        public virtual DbSet<PurchasingSuggest> PurchasingSuggest { get; set; }
        public virtual DbSet<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
        public virtual DbSet<PurchasingSuggestFile> PurchasingSuggestFile { get; set; }
        public virtual DbSet<RefCustomer> RefCustomer { get; set; }
        public virtual DbSet<RefProduct> RefProduct { get; set; }
        public virtual DbSet<VoucherAction> VoucherAction { get; set; }
        public virtual DbSet<VoucherArea> VoucherArea { get; set; }
        public virtual DbSet<VoucherAreaField> VoucherAreaField { get; set; }
        public virtual DbSet<VoucherBill> VoucherBill { get; set; }
        public virtual DbSet<VoucherField> VoucherField { get; set; }
        public virtual DbSet<VoucherType> VoucherType { get; set; }
        public virtual DbSet<VoucherTypeGlobalSetting> VoucherTypeGlobalSetting { get; set; }
        public virtual DbSet<VoucherTypeGroup> VoucherTypeGroup { get; set; }
        public virtual DbSet<VoucherTypeView> VoucherTypeView { get; set; }
        public virtual DbSet<VoucherTypeViewField> VoucherTypeViewField { get; set; }

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

                entity.Property(e => e.OrderDetailId).HasComment("VoucherValueRowId");

                entity.Property(e => e.OrderDetailQuantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.OrderDetailRequestQuantity).HasColumnType("decimal(18, 5)");

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

            modelBuilder.Entity<VoucherAction>(entity =>
            {
                entity.Property(e => e.IconName).HasMaxLength(25);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.VoucherActionCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.VoucherAction)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Action_VoucherType");
            });

            modelBuilder.Entity<VoucherArea>(entity =>
            {
                entity.Property(e => e.Columns).HasDefaultValueSql("((1))");

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.VoucherAreaCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.VoucherArea)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherArea_VoucherType");
            });

            modelBuilder.Entity<VoucherAreaField>(entity =>
            {
                entity.HasIndex(e => new { e.VoucherTypeId, e.VoucherFieldId })
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

                entity.HasOne(d => d.VoucherArea)
                    .WithMany(p => p.VoucherAreaField)
                    .HasForeignKey(d => d.VoucherAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherAreaField_VoucherArea");

                entity.HasOne(d => d.VoucherField)
                    .WithMany(p => p.VoucherAreaField)
                    .HasForeignKey(d => d.VoucherFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherAreaField_VoucherField");

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.VoucherAreaField)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherAreaField_VoucherType");
            });

            modelBuilder.Entity<VoucherBill>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_InputValueBill");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.VoucherBill)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherBill_VoucherType");
            });

            modelBuilder.Entity<VoucherField>(entity =>
            {
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RefTableCode).HasMaxLength(128);

                entity.Property(e => e.RefTableField).HasMaxLength(128);

                entity.Property(e => e.RefTableTitle).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);
            });

            modelBuilder.Entity<VoucherType>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.VoucherTypeCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.VoucherTypeGroup)
                    .WithMany(p => p.VoucherType)
                    .HasForeignKey(d => d.VoucherTypeGroupId)
                    .HasConstraintName("FK_VoucherType_VoucherTypeGroup");
            });

            modelBuilder.Entity<VoucherTypeGroup>(entity =>
            {
                entity.Property(e => e.VoucherTypeGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<VoucherTypeView>(entity =>
            {
                entity.Property(e => e.VoucherTypeViewName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.VoucherTypeView)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherTypeView_VoucherType");
            });

            modelBuilder.Entity<VoucherTypeViewField>(entity =>
            {
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RefTableCode).HasMaxLength(128);

                entity.Property(e => e.RefTableField).HasMaxLength(128);

                entity.Property(e => e.RefTableTitle).HasMaxLength(512);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.SelectFilters).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.VoucherTypeView)
                    .WithMany(p => p.VoucherTypeViewField)
                    .HasForeignKey(d => d.VoucherTypeViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VoucherTypeViewField_VoucherTypeView");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
