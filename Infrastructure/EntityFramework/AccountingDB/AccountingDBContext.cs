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
        public virtual DbSet<CategoryValue> CategoryValue { get; set; }
        public virtual DbSet<CategoryRowValue> CategoryRowValue { get; set; }
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
                entity.HasKey(v => new { v.CategoryRowId, v.CategoryFieldId });
                entity.HasOne(rv => rv.CategoryRow)
                .WithMany(r => r.CategoryRowValues)
                .HasForeignKey(rv => rv.CategoryRowId)
                .HasConstraintName("FK_CategoryRowValue_CategoryRow");
                entity.HasOne(rv => rv.CategoryField)
                .WithMany(f => f.CategoryRowValues)
                .HasForeignKey(rv => rv.CategoryFieldId)
                .HasConstraintName("FK_CategoryRowValue_CategoryField");
                entity.HasOne(rv => rv.CategoryValue)
                .WithMany(v => v.CategoryRowValues)
                .HasForeignKey(rv => rv.CategoryValueId)
                .HasConstraintName("FK_CategoryRowValue_CategoryValue");
            });

            modelBuilder.Entity<CategoryValue>(entity =>
            {
                entity.HasOne(v => v.CategoryField)
                .WithMany(f => f.CategoryValues)
                .HasForeignKey(v => v.CategoryFieldId)
                .HasConstraintName("FK_CategoryValue_CategoryField");
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

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
