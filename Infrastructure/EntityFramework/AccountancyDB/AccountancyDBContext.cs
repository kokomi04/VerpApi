using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class AccountancyDBContext : DbContext
    {
        public AccountancyDBContext()
        {
        }

        public AccountancyDBContext(DbContextOptions<AccountancyDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryField> CategoryField { get; set; }
        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputBill> InputBill { get; set; }
        public virtual DbSet<InputField> InputField { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputTypeGroup> InputTypeGroup { get; set; }
        public virtual DbSet<InputTypeView> InputTypeView { get; set; }
        public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }
        public virtual DbSet<InputValueRow> InputValueRow { get; set; }
        public virtual DbSet<OutSideDataConfig> OutSideDataConfig { get; set; }
        public virtual DbSet<Tet> Tet { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryCode)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<CategoryField>(entity =>
            {
                entity.Property(e => e.CategoryFieldName)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Filters).HasMaxLength(512);

                entity.Property(e => e.IsShowList)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsShowSearchTable)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.RefTableCode)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefTableField)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefTableTitle)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RegularExpression)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryField)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryField_Category");
            });

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

            modelBuilder.Entity<InputValueRow>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_@_InputValueRow");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.Property(e => e.Attachment).HasMaxLength(512);

                entity.Property(e => e.BoPhan).HasColumnName("Bo_phan");

                entity.Property(e => e.CongTrinh).HasColumnName("Cong_trinh");

                entity.Property(e => e.ConstrainTyGia).HasColumnName("Constrain_Ty_gia");

                entity.Property(e => e.DiaChi)
                    .HasColumnName("Dia_chi")
                    .HasMaxLength(512);

                entity.Property(e => e.DienGiai)
                    .HasColumnName("DIEN_GIAI")
                    .HasMaxLength(512);

                entity.Property(e => e.DonGia)
                    .HasColumnName("Don_gia")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.DonGiaDvt2)
                    .HasColumnName("Don_gia_DVT2")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Dvt)
                    .HasColumnName("DVT")
                    .HasMaxLength(512);

                entity.Property(e => e.Dvt2)
                    .HasColumnName("DVT2")
                    .HasMaxLength(512);

                entity.Property(e => e.GhiChu)
                    .HasColumnName("Ghi_chu")
                    .HasMaxLength(512);

                entity.Property(e => e.InputBillFId).HasColumnName("InputBill_F_Id");

                entity.Property(e => e.KheUocVay).HasColumnName("Khe_uoc_vay");

                entity.Property(e => e.KhoC).HasColumnName("Kho_C");

                entity.Property(e => e.KhoanMucCp).HasColumnName("Khoan_muc_CP");

                entity.Property(e => e.KhoanMucTc).HasColumnName("Khoan_muc_TC");

                entity.Property(e => e.KyHieuHd)
                    .HasColumnName("Ky_hieu_hd")
                    .HasMaxLength(512);

                entity.Property(e => e.LoaiTien).HasColumnName("Loai_tien");

                entity.Property(e => e.MaChuongNsnn).HasColumnName("Ma_chuong_NSNN");

                entity.Property(e => e.MaCt)
                    .HasColumnName("MA_CT")
                    .HasMaxLength(512);

                entity.Property(e => e.MaKh0).HasColumnName("Ma_kh0");

                entity.Property(e => e.MaKh1).HasColumnName("Ma_kh1");

                entity.Property(e => e.MaKh3).HasColumnName("Ma_kh3");

                entity.Property(e => e.MaKhc0).HasColumnName("Ma_khc0");

                entity.Property(e => e.MaLinkHd)
                    .HasColumnName("Ma_link_hd")
                    .HasMaxLength(512);

                entity.Property(e => e.MaLsx)
                    .HasColumnName("Ma_LSX")
                    .HasMaxLength(512);

                entity.Property(e => e.MaMucNsnn).HasColumnName("Ma_muc_NSNN");

                entity.Property(e => e.MaTscd).HasColumnName("Ma_TSCD");

                entity.Property(e => e.MaVthhtp).HasColumnName("Ma_VTHHTP");

                entity.Property(e => e.MauHd)
                    .HasColumnName("Mau_hd")
                    .HasMaxLength(512);

                entity.Property(e => e.NgayCt).HasColumnName("Ngay_ct");

                entity.Property(e => e.NgayHd).HasColumnName("Ngay_hd");

                entity.Property(e => e.NgoaiTe0)
                    .HasColumnName("Ngoai_te0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.NoiDung)
                    .HasColumnName("Noi_dung")
                    .HasMaxLength(512);

                entity.Property(e => e.OngBa)
                    .HasColumnName("Ong_ba")
                    .HasMaxLength(512);

                entity.Property(e => e.OrderCode)
                    .HasColumnName("Order_Code")
                    .HasMaxLength(512);

                entity.Property(e => e.PhanXuong).HasColumnName("Phan_xuong");

                entity.Property(e => e.PoCode)
                    .HasColumnName("PO_Code")
                    .HasMaxLength(512);

                entity.Property(e => e.SeriHd)
                    .HasColumnName("Seri_hd")
                    .HasMaxLength(512);

                entity.Property(e => e.SlOd)
                    .HasColumnName("SL_OD")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SlPo)
                    .HasColumnName("SL_PO")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SlReq)
                    .HasColumnName("SL_Req")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SlTonKho)
                    .HasColumnName("SL_ton_kho")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SoCt)
                    .HasColumnName("So_ct")
                    .HasMaxLength(512);

                entity.Property(e => e.SoLuong)
                    .HasColumnName("So_luong")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SoLuongDv2)
                    .HasColumnName("So_luong_DV2")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Stt)
                    .HasColumnName("STT")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TenKh0)
                    .HasColumnName("Ten_kh0")
                    .HasMaxLength(512);

                entity.Property(e => e.TenKh1)
                    .HasColumnName("Ten_kh1")
                    .HasMaxLength(512);

                entity.Property(e => e.TenKh3)
                    .HasColumnName("Ten_kh3")
                    .HasMaxLength(512);

                entity.Property(e => e.TenKhc0)
                    .HasColumnName("Ten_khc0")
                    .HasMaxLength(512);

                entity.Property(e => e.TenTscd)
                    .HasColumnName("Ten_TSCD")
                    .HasMaxLength(512);

                entity.Property(e => e.TenVthhtp)
                    .HasColumnName("Ten_VTHHTP")
                    .HasMaxLength(512);

                entity.Property(e => e.ThueSuatVat).HasColumnName("Thue_suat_VAT");

                entity.Property(e => e.ThueSuatXnk)
                    .HasColumnName("Thue_suat_xnk")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TkCo0).HasColumnName("TK_co0");

                entity.Property(e => e.TkCo1).HasColumnName("TK_co1");

                entity.Property(e => e.TkCo2).HasColumnName("TK_co2");

                entity.Property(e => e.TkCo3).HasColumnName("TK_co3");

                entity.Property(e => e.TkNo0).HasColumnName("Tk_no0");

                entity.Property(e => e.TkNo1).HasColumnName("Tk_no1");

                entity.Property(e => e.TkNo2).HasColumnName("Tk_no2");

                entity.Property(e => e.TkNo3).HasColumnName("Tk_no3");

                entity.Property(e => e.TkThuKbnn).HasColumnName("TK_thu_KBNN");

                entity.Property(e => e.TknhDn).HasColumnName("TKNH_DN");

                entity.Property(e => e.TknhDt).HasColumnName("TKNH_DT");

                entity.Property(e => e.TongCong)
                    .HasColumnName("Tong_cong")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TongNgoaiTe0)
                    .HasColumnName("Tong_ngoai_te0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TongTienHang)
                    .HasColumnName("Tong_tien_hang")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TongVnd0)
                    .HasColumnName("Tong_vnd0")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TongVnd1)
                    .HasColumnName("Tong_vnd1")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.TyGia)
                    .HasColumnName("Ty_gia")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Vnd0).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vnd1).HasColumnType("decimal(18, 5)");
            });

            modelBuilder.Entity<OutSideDataConfig>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PK__OutSideD__19093A0B4E7BC7E8");

                entity.Property(e => e.CategoryId).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Key)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ParentKey)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithOne(p => p.OutSideDataConfig)
                    .HasForeignKey<OutSideDataConfig>(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutSideDataConfig_Category");
            });

            modelBuilder.Entity<Tet>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK__tet__2C6EC723091E16CD");

                entity.ToTable("tet");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
