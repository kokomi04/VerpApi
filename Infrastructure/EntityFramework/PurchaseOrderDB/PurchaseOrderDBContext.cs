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
        public virtual DbSet<SaleBill> SaleBill { get; set; }
        public virtual DbSet<VoucherAction> VoucherAction { get; set; }
        public virtual DbSet<VoucherArea> VoucherArea { get; set; }
        public virtual DbSet<VoucherAreaField> VoucherAreaField { get; set; }
        public virtual DbSet<VoucherField> VoucherField { get; set; }
        public virtual DbSet<VoucherType> VoucherType { get; set; }
        public virtual DbSet<VoucherTypeGroup> VoucherTypeGroup { get; set; }
        public virtual DbSet<VoucherTypeView> VoucherTypeView { get; set; }
        public virtual DbSet<VoucherTypeViewField> VoucherTypeViewField { get; set; }
        public virtual DbSet<VoucherValueRow> VoucherValueRow { get; set; }

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

            modelBuilder.Entity<SaleBill>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_InputValueBill");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.HasOne(d => d.VoucherType)
                    .WithMany(p => p.SaleBill)
                    .HasForeignKey(d => d.VoucherTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleBill_VoucherType");
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

            modelBuilder.Entity<VoucherField>(entity =>
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

            modelBuilder.Entity<VoucherValueRow>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_@_InputValueRow");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.Property(e => e.Attachment)
                    .HasColumnName("attachment")
                    .HasMaxLength(1);

                entity.Property(e => e.BoPhan).HasColumnName("bo_phan");

                entity.Property(e => e.DienGiai)
                    .HasColumnName("dien_giai")
                    .HasMaxLength(512);

                entity.Property(e => e.Dkgh)
                    .HasColumnName("dkgh")
                    .HasMaxLength(512);

                entity.Property(e => e.Dktt)
                    .HasColumnName("dktt")
                    .HasMaxLength(1);

                entity.Property(e => e.DonGia0)
                    .HasColumnName("don_gia0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.DonGiaDv2)
                    .HasColumnName("don_gia_dv2")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.DonGiaNt)
                    .HasColumnName("don_gia_nt")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Dvt).HasColumnName("dvt");

                entity.Property(e => e.GhiChu)
                    .HasColumnName("ghi_chu")
                    .HasMaxLength(512);

                entity.Property(e => e.Kh0)
                    .HasColumnName("kh0")
                    .HasMaxLength(64);

                entity.Property(e => e.KhNguoiLh)
                    .HasColumnName("kh_nguoi_lh")
                    .HasMaxLength(512);

                entity.Property(e => e.KhVt)
                    .HasColumnName("kh_vt")
                    .HasMaxLength(1);

                entity.Property(e => e.LoaiTien).HasColumnName("loai_tien");

                entity.Property(e => e.MaBgBh)
                    .HasColumnName("ma_bg_bh")
                    .HasMaxLength(512);

                entity.Property(e => e.MaVthhtpYc)
                    .HasColumnName("ma_vthhtp_yc")
                    .HasMaxLength(512);

                entity.Property(e => e.NgayCt).HasColumnName("ngay_ct");

                entity.Property(e => e.NgayGh).HasColumnName("ngay_gh");

                entity.Property(e => e.NgoaiTe0)
                    .HasColumnName("ngoai_te0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.NguoiPhuTrach)
                    .HasColumnName("nguoi_phu_trach")
                    .HasMaxLength(512);

                entity.Property(e => e.NoiDung)
                    .HasColumnName("noi_dung")
                    .HasMaxLength(512);

                entity.Property(e => e.OngBa)
                    .HasColumnName("ong_ba")
                    .HasMaxLength(512);

                entity.Property(e => e.SaleBillFId).HasColumnName("SaleBill_F_Id");

                entity.Property(e => e.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(512);

                entity.Property(e => e.SoLuong)
                    .HasColumnName("so_luong")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SoLuongDv2).HasColumnName("so_luong_dv2");

                entity.Property(e => e.Stt).HasColumnName("stt");

                entity.Property(e => e.SumVnd0)
                    .HasColumnName("sum_vnd0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SumVnd1)
                    .HasColumnName("sum_vnd1")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SumVnd2)
                    .HasColumnName("sum_vnd2")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SumVnd3)
                    .HasColumnName("sum_vnd3")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SumVnd4)
                    .HasColumnName("sum_vnd4")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SumVnd5)
                    .HasColumnName("sum_vnd5")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SystemLog).HasMaxLength(128);

                entity.Property(e => e.TheTich)
                    .HasColumnName("the_tich")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Thhl)
                    .HasColumnName("thhl")
                    .HasMaxLength(128);

                entity.Property(e => e.ThongTinVthhtp)
                    .HasColumnName("thong_tin_vthhtp")
                    .HasMaxLength(512);

                entity.Property(e => e.ThueSuatVat)
                    .HasColumnName("thue_suat_vat")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ThueSuatXnk).HasColumnName("thue_suat_xnk");

                entity.Property(e => e.TkCo)
                    .HasColumnName("tk_co")
                    .HasMaxLength(1);

                entity.Property(e => e.TongTheTich)
                    .HasColumnName("tong_the_tich")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TtVthhtpYc)
                    .HasColumnName("tt_vthhtp_yc")
                    .HasMaxLength(2048);

                entity.Property(e => e.TyGia)
                    .HasColumnName("ty_gia")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vnd0)
                    .HasColumnName("vnd0")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vnd1)
                    .HasColumnName("vnd1")
                    .HasColumnType("decimal(8, 0)");

                entity.Property(e => e.Vnd3)
                    .HasColumnName("vnd3")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Vthhtp).HasColumnName("vthhtp");

                entity.Property(e => e.VthhtpDvt2).HasColumnName("vthhtp_dvt2");

                entity.Property(e => e.VthhtpYc)
                    .HasColumnName("vthhtp_yc")
                    .HasMaxLength(512);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
