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

        //public virtual DbSet<CategoryValue> CategoryValue { get; set; }
        public virtual DbSet<CategoryRowValue> CategoryRowValue { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputValueBill> InputValueBill { get; set; }
        public virtual DbSet<InputValueRow> InputValueRow { get; set; }
        public virtual DbSet<InputValueRowVersion> InputValueRowVersion { get; set; }

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
            });

            modelBuilder.Entity<CategoryRow>(entity =>
            {
                entity.HasOne(r => r.Category)
                .WithMany(c => c.CategoryRows)
                .HasForeignKey(r => r.CategoryId)
                .HasConstraintName("FK_CategoryRow_Category");
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
            });

            //modelBuilder.Entity<CategoryValue>(entity =>
            //{
            //    entity.HasOne(v => v.CategoryField)
            //    .WithMany(f => f.CategoryValues)
            //    .HasForeignKey(v => v.CategoryFieldId)
            //    .HasConstraintName("FK_CategoryValue_CategoryField");
            //});

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

            });

            modelBuilder.Entity<InputArea>(entity =>
            {
                entity.HasOne(a => a.InputType)
                .WithMany(t => t.InputAreas)
                .HasForeignKey(a => a.InputTypeId)
                .HasConstraintName("FK_InputArea_InputType");
            });

            modelBuilder.Entity<InputAreaField>(entity =>
            {
                entity.HasKey(f => new { f.InputAreaId, f.FieldIndex });
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
            });

            modelBuilder.Entity<InputValueRowVersion>(entity =>
            {
                entity.HasOne(rv => rv.InputValueRow)
                .WithMany(r => r.InputValueRowVersions)
                .HasForeignKey(rv => rv.InputValueRowId)
                .HasConstraintName("FK_InputValueRowVersion_InputValueRow");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
