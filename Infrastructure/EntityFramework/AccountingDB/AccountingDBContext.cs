using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class AccountingDBContext : DbContext
    {
        public virtual DbSet<AccountingAccount> AccountingAccount { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryRow> CategoryRow { get; set; }
        public virtual DbSet<DataType> DataType { get; set; }
        public virtual DbSet<FormType> FormType { get; set; }
        public virtual DbSet<CategoryField> CategoryField { get; set; }
        public virtual DbSet<OutSideDataConfig> OutSideDataConfig { get; set; }
        public virtual DbSet<CategoryRowValue> CategoryRowValue { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputTypeView> InputTypeView { get; set; }
        public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }
        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputAreaFieldStyle> InputAreaFieldStyle { get; set; }
        public virtual DbSet<InputValueBill> InputValueBill { get; set; }
        public virtual DbSet<InputValueRow> InputValueRow { get; set; }
        public virtual DbSet<InputValueRowVersion> InputValueRowVersion { get; set; }
        public virtual DbSet<InputValueRowVersionNumber> InputValueRowVersionNumber { get; set; }


        public AccountingDBContext()
        {
        }

        public AccountingDBContext(DbContextOptions<AccountingDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountingAccount>(entity =>
            {
                entity.HasOne(a => a.ParentAccountingAccount)
                .WithMany(a => a.SubAccountingAccount)
                .HasForeignKey(a => a.ParentAccountingAccountId)
                .HasConstraintName("FK_AccountingAccount_Relation");
            });


            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .HasConstraintName("FK_Category_Relation");
                entity.HasIndex(c => c.ParentId).HasName("IDX_Category_Relation_FK");
            });

            modelBuilder.Entity<CategoryRow>(entity =>
            {
                entity.HasOne(r => r.Category)
                .WithMany(c => c.CategoryRows)
                .HasForeignKey(r => r.CategoryId)
                .HasConstraintName("FK_CategoryRow_Category");
                entity.HasOne(r => r.ParentCategoryRow)
                .WithMany(c => c.ChildCategoryRows)
                .HasForeignKey(r => r.ParentCategoryRowId)
                .HasConstraintName("FK_CategoryRow_Relation");
                entity.HasIndex(r => r.CategoryId).HasName("IDX_CategoryRow_Category_FK");
            });

            modelBuilder.Entity<CategoryRowValue>(entity =>
            {
                entity.HasOne(rv => rv.CategoryRow)
                .WithMany(r => r.CategoryRowValues)
                .HasForeignKey(rv => rv.CategoryRowId)
                .HasConstraintName("FK_CategoryRowValue_CategoryRow");
                entity.HasOne(rv => rv.CategoryField)
                .WithMany(f => f.CategoryRowValues)
                .HasForeignKey(rv => rv.CategoryFieldId)
                .HasConstraintName("FK_CategoryRowValue_CategoryField");
                entity.HasOne(rv => rv.SourceCategoryRowValue)
                .WithMany(v => v.DestCategoryRowValue)
                .HasForeignKey(rv => rv.ReferenceCategoryRowValueId)
                .HasConstraintName("FK_CategoryRowValue_Relation");
                entity.HasIndex(rv => rv.CategoryRowId).HasName("IDX_CategoryRowValue_CategoryRow_FK");
                entity.HasIndex(rv => rv.CategoryFieldId).HasName("IDX_CategoryRowValue_CategoryField_FK");
            });

            modelBuilder.Entity<CategoryField>(entity =>
            {
                entity.HasOne(f => f.Category)
                .WithMany(c => c.CategoryFields)
                .HasForeignKey(c => c.CategoryId)
                .HasConstraintName("FK_CategoryField_Category");
                entity.HasOne(f => f.DataType)
                .WithMany(d => d.CategoryFields)
                .HasForeignKey(f => f.DataTypeId)
                .HasConstraintName("FK_CategoryField_DataType");
                entity.HasOne(f => f.FormType)
                .WithMany(f => f.CategoryFields)
                .HasForeignKey(f => f.FormTypeId)
                .HasConstraintName("FK_CategoryField_FormType");
                entity.HasOne(f => f.SourceCategoryField)
                .WithMany(f => f.DestCategoryFields)
                .HasForeignKey(c => c.ReferenceCategoryFieldId)
                .HasConstraintName("FK_CategoryField_Relation");
                entity.HasOne(f => f.SourceCategoryTitleField)
                .WithMany(f => f.DestCategoryTitleFields)
                .HasForeignKey(c => c.ReferenceCategoryTitleFieldId)
                .HasConstraintName("FK_CategoryTitleField_Relation");

                entity.HasIndex(f => f.CategoryId).HasName("IDX_CategoryField_Category_FK");
                entity.HasIndex(f => f.DataTypeId).HasName("IDX_CategoryField_DataType_FK");
                entity.HasIndex(f => f.FormTypeId).HasName("IDX_CategoryField_FormType_FK");
                entity.HasIndex(f => f.ReferenceCategoryFieldId).HasName("IDX_CategoryField_Relation_FK");
                entity.HasIndex(f => f.ReferenceCategoryTitleFieldId).HasName("IDX_CategoryTitleField_Relation_FK");

            });

            modelBuilder.Entity<OutSideDataConfig>(entity =>
            {
                entity.HasKey(cf => cf.CategoryId);
                entity.HasOne(cf => cf.Category)
                .WithOne(c => c.OutSideDataConfig)
                .HasForeignKey<OutSideDataConfig>(cf => cf.CategoryId)
                .HasConstraintName("FK_OutSideDataConfig_Category");
            });


            //modelBuilder.Entity<InputTypeView>(entity =>
            //{
            //    entity.HasOne(d => d.InputType)
            //        .WithMany(p => p.InputTypeView)
            //        .HasForeignKey(d => d.InputTypeId)
            //        .OnDelete(DeleteBehavior.ClientSetNull)
            //        .HasConstraintName("FK_InputTypeView_InputType");
            //});

            //modelBuilder.Entity<InputTypeViewField>(entity =>
            //{
            //    entity.HasKey(e => new { e.InputTypeViewId, e.InputAreaFieldId });

            //    entity.Property(e => e.DefaultValue).HasMaxLength(512);

            //    entity.HasOne(d => d.InputAreaField)
            //        .WithMany(p => p.InputTypeViewField)
            //        .HasForeignKey(d => d.InputAreaFieldId)
            //        .OnDelete(DeleteBehavior.ClientSetNull)
            //        .HasConstraintName("FK_InputTypeViewField_InputAreaField");

            //    entity.HasOne(d => d.InputTypeView)
            //        .WithMany(p => p.InputTypeViewField)
            //        .HasForeignKey(d => d.InputTypeViewId)
            //        .OnDelete(DeleteBehavior.ClientSetNull)
            //        .HasConstraintName("FK_InputTypeViewField_InputTypeView");
            //});

            modelBuilder.Entity<InputArea>(entity =>
            {
                entity.HasOne(a => a.InputType)
                .WithMany(t => t.InputAreas)
                .HasForeignKey(a => a.InputTypeId)
                .HasConstraintName("FK_InputArea_InputType");
            });

            modelBuilder.Entity<InputAreaField>(entity =>
            {
                entity.HasOne(f => f.InputArea)
                .WithMany(f => f.InputAreaFields)
                .HasForeignKey(f => f.InputAreaId)
                .HasConstraintName("FK_InputAreaField_InputArea");
                entity.HasOne(f => f.DataType)
                .WithMany(d => d.InputAreaFields)
                .HasForeignKey(f => f.DataTypeId)
                .HasConstraintName("FK_InputAreaField_DataType");
                entity.HasOne(f => f.FormType)
                .WithMany(f => f.InputAreaFields)
                .HasForeignKey(f => f.FormTypeId)
                .HasConstraintName("FK_InputAreaField_FormType");
                entity.HasOne(f => f.SourceCategoryField)
                .WithMany(f => f.InputAreaFields)
                .HasForeignKey(c => c.ReferenceCategoryFieldId)
                .HasConstraintName("FK_InputAreaField_CategoryField");
                entity.HasOne(f => f.SourceCategoryTitleField)
                .WithMany(f => f.InputAreaTitleFields)
                .HasForeignKey(c => c.ReferenceCategoryTitleFieldId)
                .HasConstraintName("FK_InputAreaField_CategoryTitleField");
            });

            modelBuilder.Entity<InputValueRow>(entity =>
            {
                entity.HasOne(r => r.InputValueBill)
                .WithMany(b => b.InputValueRows)
                .HasForeignKey(r => r.InputValueBillId)
                .HasConstraintName("FK_InputValueRow_InputValueBill");
                entity.HasOne(r => r.InputArea)
                .WithMany(a => a.InputValueRows)
                .HasForeignKey(r => r.InputAreaId)
                .HasConstraintName("FK_InputValueRow_InputArea");
            });

            modelBuilder.Entity<InputValueBill>(entity =>
            {
                entity.HasOne(b => b.InputType)
               .WithMany(i => i.InputValueBills)
               .HasForeignKey(b => b.InputTypeId)
               .HasConstraintName("FK_InputValueBill_InputType");

            });


            modelBuilder.Entity<InputValueRowVersion>(entity =>
            {
                entity.HasOne(rv => rv.InputValueRow)
                .WithMany(r => r.InputValueRowVersions)
                .HasForeignKey(rv => rv.InputValueRowId)
                .HasConstraintName("FK_InputValueRowVersion_InputValueRow");
            });

            modelBuilder.Entity<InputValueRowVersionNumber>(entity =>
            {
                entity.HasKey(rvn => rvn.InputValueRowVersionId);
                entity.HasOne(rvn => rvn.InputValueRowVersion)
                .WithOne(rv => rv.InputValueRowVersionNumber)
                .HasForeignKey<InputValueRowVersionNumber>(rvn => rvn.InputValueRowVersionId)
                .HasConstraintName("FK_InputValueRowVersionNumber_InputValueRowVersion");
            });

            modelBuilder.Entity<InputAreaFieldStyle>(entity =>
            {
                entity.HasKey(fs => fs.InputAreaFieldId);
                entity.HasOne(fs => fs.InputAreaField)
                .WithOne(f => f.InputAreaFieldStyle)
                .HasForeignKey<InputAreaFieldStyle>(fs => fs.InputAreaFieldId)
                .HasConstraintName("FK_InputAreaFieldStyle_InputAreaField");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
