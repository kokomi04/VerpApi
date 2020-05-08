using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class AccountingDBContext : DbContext
    {
        public AccountingDBContext()
        {
        }

        public AccountingDBContext(DbContextOptions<AccountingDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountingAccount> AccountingAccount { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryField> CategoryField { get; set; }
        public virtual DbSet<CategoryRow> CategoryRow { get; set; }
        public virtual DbSet<CategoryRowValue> CategoryRowValue { get; set; }
        public virtual DbSet<DataType> DataType { get; set; }
        public virtual DbSet<FormType> FormType { get; set; }
        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputAreaFieldStyle> InputAreaFieldStyle { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputTypeView> InputTypeView { get; set; }
        public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }
        public virtual DbSet<InputValueBill> InputValueBill { get; set; }
        public virtual DbSet<InputValueRow> InputValueRow { get; set; }
        public virtual DbSet<InputValueRowVersion> InputValueRowVersion { get; set; }
        public virtual DbSet<InputValueRowVersionNumber> InputValueRowVersionNumber { get; set; }
        public virtual DbSet<OutSideDataConfig> OutSideDataConfig { get; set; }
        public virtual DbSet<Sysdiagrams> Sysdiagrams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountingAccount>(entity =>
            {
                entity.Property(e => e.AccountNameEn).HasMaxLength(128);

                entity.Property(e => e.AccountNameVi).HasMaxLength(128);

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasComment("ngoại tệ");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.IsBranch).HasComment("TGHT tỷ giá hạch toán");

                entity.Property(e => e.IsCorp).HasComment("Tổng công ty");

                entity.Property(e => e.IsForeignCurrency).HasComment("Ngoại tệ");

                entity.Property(e => e.IsLiability).HasComment("Công nợ");

                entity.Property(e => e.IsStock).HasComment("Kho");

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.ParentAccountingAccount)
                    .WithMany(p => p.InverseParentAccountingAccount)
                    .HasForeignKey(d => d.ParentAccountingAccountId)
                    .HasConstraintName("FK_AccountingAccount_Relation");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.ParentId)
                    .HasName("IDX_Category_Relation_FK");

                entity.Property(e => e.CategoryCode)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Category_Relation");
            });

            modelBuilder.Entity<CategoryField>(entity =>
            {
                entity.HasIndex(e => e.CategoryId)
                    .HasName("IDX_CategoryField_Category_FK");

                entity.HasIndex(e => e.DataTypeId)
                    .HasName("IDX_CategoryField_DataType_FK");

                entity.HasIndex(e => e.FormTypeId)
                    .HasName("IDX_CategoryField_FormType_FK");

                entity.HasIndex(e => e.ReferenceCategoryFieldId)
                    .HasName("IDX_CategoryField_Relation_FK");

                entity.HasIndex(e => e.ReferenceCategoryTitleFieldId)
                    .HasName("IDX_CategoryTitleField_Relation_FK");

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

                entity.Property(e => e.IsTreeViewKey).HasColumnName("IsTreeViewKey ");

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

                entity.HasOne(d => d.DataType)
                    .WithMany(p => p.CategoryField)
                    .HasForeignKey(d => d.DataTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryField_DataType");

                entity.HasOne(d => d.FormType)
                    .WithMany(p => p.CategoryField)
                    .HasForeignKey(d => d.FormTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryField_FormType");

                entity.HasOne(d => d.ReferenceCategoryField)
                    .WithMany(p => p.InverseReferenceCategoryField)
                    .HasForeignKey(d => d.ReferenceCategoryFieldId)
                    .HasConstraintName("FK_CategoryField_Relation");

                entity.HasOne(d => d.ReferenceCategoryTitleField)
                    .WithMany(p => p.InverseReferenceCategoryTitleField)
                    .HasForeignKey(d => d.ReferenceCategoryTitleFieldId)
                    .HasConstraintName("FK_CategoryTitleField_Relation");
            });

            modelBuilder.Entity<CategoryRow>(entity =>
            {
                entity.HasIndex(e => e.CategoryId)
                    .HasName("IDX_CategoryRow_Category_FK");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryRow)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryRow_Category");

                entity.HasOne(d => d.ParentCategoryRow)
                    .WithMany(p => p.InverseParentCategoryRow)
                    .HasForeignKey(d => d.ParentCategoryRowId)
                    .HasConstraintName("FK_CategoryRow_Relation");
            });

            modelBuilder.Entity<CategoryRowValue>(entity =>
            {
                entity.HasIndex(e => e.CategoryFieldId)
                    .HasName("IDX_CategoryRowValue_CategoryField_FK");

                entity.HasIndex(e => e.CategoryRowId)
                    .HasName("IDX_CategoryRowValue_CategoryRow_FK");

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Value).HasMaxLength(512);

                entity.HasOne(d => d.CategoryField)
                    .WithMany(p => p.CategoryRowValue)
                    .HasForeignKey(d => d.CategoryFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryRowValue_CategoryField");

                entity.HasOne(d => d.CategoryRow)
                    .WithMany(p => p.CategoryRowValue)
                    .HasForeignKey(d => d.CategoryRowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryRowValue_CategoryRow");

                entity.HasOne(d => d.ReferenceCategoryRowValue)
                    .WithMany(p => p.InverseReferenceCategoryRowValue)
                    .HasForeignKey(d => d.ReferenceCategoryRowValueId)
                    .HasConstraintName("FK_CategoryRowValue_Relation");
            });

            modelBuilder.Entity<DataType>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.RegularExpression)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            });

            modelBuilder.Entity<FormType>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
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
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Filters).HasMaxLength(512);

                entity.Property(e => e.IsListFilter)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.DataType)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.DataTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_DataType");

                entity.HasOne(d => d.FormType)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.FormTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_FormType");

                entity.HasOne(d => d.InputArea)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.InputAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_InputArea");

                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputAreaField)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaField_InputType");

                entity.HasOne(d => d.ReferenceCategoryField)
                    .WithMany(p => p.InputAreaFieldReferenceCategoryField)
                    .HasForeignKey(d => d.ReferenceCategoryFieldId)
                    .HasConstraintName("FK_InputAreaField_CategoryField");

                entity.HasOne(d => d.ReferenceCategoryTitleField)
                    .WithMany(p => p.InputAreaFieldReferenceCategoryTitleField)
                    .HasForeignKey(d => d.ReferenceCategoryTitleFieldId)
                    .HasConstraintName("FK_InputAreaField_CategoryTitleField");
            });

            modelBuilder.Entity<InputAreaFieldStyle>(entity =>
            {
                entity.HasKey(e => e.InputAreaFieldId);

                entity.Property(e => e.InputAreaFieldId).ValueGeneratedNever();

                entity.Property(e => e.Column).HasDefaultValueSql("((1))");

                entity.Property(e => e.InputStyleJson).HasMaxLength(512);

                entity.Property(e => e.OnBlur).HasMaxLength(512);

                entity.Property(e => e.OnChange).HasMaxLength(512);

                entity.Property(e => e.OnFocus).HasMaxLength(512);

                entity.Property(e => e.OnKeydown).HasMaxLength(512);

                entity.Property(e => e.OnKeypress).HasMaxLength(512);

                entity.Property(e => e.TitleStyleJson).HasMaxLength(512);

                entity.HasOne(d => d.InputAreaField)
                    .WithOne(p => p.InputAreaFieldStyle)
                    .HasForeignKey<InputAreaFieldStyle>(d => d.InputAreaFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAreaFieldStyle_InputAreaField");
            });

            modelBuilder.Entity<InputType>(entity =>
            {
                entity.Property(e => e.InputTypeCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(128);
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

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.SelectFilters).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.DataType)
                    .WithMany(p => p.InputTypeViewField)
                    .HasForeignKey(d => d.DataTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputTypeViewField_DataType");

                entity.HasOne(d => d.FormType)
                    .WithMany(p => p.InputTypeViewField)
                    .HasForeignKey(d => d.FormTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputTypeViewField_FormType");

                entity.HasOne(d => d.InputTypeView)
                    .WithMany(p => p.InputTypeViewField)
                    .HasForeignKey(d => d.InputTypeViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputTypeViewField_InputTypeView");

                entity.HasOne(d => d.ReferenceCategoryField)
                    .WithMany(p => p.InputTypeViewFieldReferenceCategoryField)
                    .HasForeignKey(d => d.ReferenceCategoryFieldId)
                    .HasConstraintName("FK_InputTypeViewField_CategoryField");

                entity.HasOne(d => d.ReferenceCategory)
                    .WithMany(p => p.InputTypeViewField)
                    .HasForeignKey(d => d.ReferenceCategoryId)
                    .HasConstraintName("FK_InputTypeViewField_Category");

                entity.HasOne(d => d.ReferenceCategoryTitleField)
                    .WithMany(p => p.InputTypeViewFieldReferenceCategoryTitleField)
                    .HasForeignKey(d => d.ReferenceCategoryTitleFieldId)
                    .HasConstraintName("FK_InputTypeViewField_CategoryField1");
            });

            modelBuilder.Entity<InputValueBill>(entity =>
            {
                entity.HasOne(d => d.InputType)
                    .WithMany(p => p.InputValueBill)
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueBill_InputType");
            });

            modelBuilder.Entity<InputValueRow>(entity =>
            {
                entity.HasOne(d => d.InputArea)
                    .WithMany(p => p.InputValueRow)
                    .HasForeignKey(d => d.InputAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueRow_InputArea");

                entity.HasOne(d => d.InputValueBill)
                    .WithMany(p => p.InputValueRow)
                    .HasForeignKey(d => d.InputValueBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueRow_InputValueBill");
            });

            modelBuilder.Entity<InputValueRowVersion>(entity =>
            {
                entity.Property(e => e.Field0).HasMaxLength(512);

                entity.Property(e => e.Field1).HasMaxLength(512);

                entity.Property(e => e.Field10).HasMaxLength(512);

                entity.Property(e => e.Field11).HasMaxLength(512);

                entity.Property(e => e.Field12).HasMaxLength(512);

                entity.Property(e => e.Field13).HasMaxLength(512);

                entity.Property(e => e.Field14).HasMaxLength(512);

                entity.Property(e => e.Field15).HasMaxLength(512);

                entity.Property(e => e.Field16).HasMaxLength(512);

                entity.Property(e => e.Field17).HasMaxLength(512);

                entity.Property(e => e.Field18).HasMaxLength(512);

                entity.Property(e => e.Field19).HasMaxLength(512);

                entity.Property(e => e.Field2).HasMaxLength(512);

                entity.Property(e => e.Field20).HasMaxLength(512);

                entity.Property(e => e.Field3).HasMaxLength(512);

                entity.Property(e => e.Field4).HasMaxLength(512);

                entity.Property(e => e.Field5).HasMaxLength(512);

                entity.Property(e => e.Field6).HasMaxLength(512);

                entity.Property(e => e.Field7).HasMaxLength(512);

                entity.Property(e => e.Field8).HasMaxLength(512);

                entity.Property(e => e.Field9).HasMaxLength(512);

                entity.HasOne(d => d.InputValueRow)
                    .WithMany(p => p.InputValueRowVersion)
                    .HasForeignKey(d => d.InputValueRowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueRowVersion_InputValueRow");
            });

            modelBuilder.Entity<InputValueRowVersionNumber>(entity =>
            {
                entity.HasKey(e => e.InputValueRowVersionId);

                entity.Property(e => e.InputValueRowVersionId).ValueGeneratedNever();

                entity.HasOne(d => d.InputValueRowVersion)
                    .WithOne(p => p.InputValueRowVersionNumber)
                    .HasForeignKey<InputValueRowVersionNumber>(d => d.InputValueRowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputValueRowVersionNumber_InputValueRowVersion");
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

                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithOne(p => p.OutSideDataConfig)
                    .HasForeignKey<OutSideDataConfig>(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutSideDataConfig_Category");
            });

            modelBuilder.Entity<Sysdiagrams>(entity =>
            {
                entity.HasKey(e => e.DiagramId)
                    .HasName("PK__sysdiagr__C2B05B61F1DB3560");

                entity.ToTable("sysdiagrams");

                entity.HasIndex(e => new { e.PrincipalId, e.Name })
                    .HasName("UK_principal_name")
                    .IsUnique();

                entity.Property(e => e.DiagramId).HasColumnName("diagram_id");

                entity.Property(e => e.Definition).HasColumnName("definition");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.PrincipalId).HasColumnName("principal_id");

                entity.Property(e => e.Version).HasColumnName("version");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
