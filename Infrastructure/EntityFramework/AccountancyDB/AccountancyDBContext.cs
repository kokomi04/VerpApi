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

        public virtual DbSet<InputAction> InputAction { get; set; }
        public virtual DbSet<InputArea> InputArea { get; set; }
        public virtual DbSet<InputAreaField> InputAreaField { get; set; }
        public virtual DbSet<InputBill> InputBill { get; set; }
        public virtual DbSet<InputField> InputField { get; set; }
        public virtual DbSet<InputType> InputType { get; set; }
        public virtual DbSet<InputTypeGroup> InputTypeGroup { get; set; }
        public virtual DbSet<InputTypeView> InputTypeView { get; set; }
        public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }
        public virtual DbSet<OutsideImportMapping> OutsideImportMapping { get; set; }
        public virtual DbSet<OutsideImportMappingFunction> OutsideImportMappingFunction { get; set; }
        public virtual DbSet<OutsideImportMappingObject> OutsideImportMappingObject { get; set; }
        public virtual DbSet<PrintConfig> PrintConfig { get; set; }
        public virtual DbSet<ProgramingFunction> ProgramingFunction { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InputAction>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.IconName).HasMaxLength(25);

                entity.Property(e => e.InputActionCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.InputActionId).ValueGeneratedOnAdd();

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.InputType)
                    .WithMany()
                    .HasForeignKey(d => d.InputTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InputAction_InputType");
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

            modelBuilder.Entity<OutsideImportMapping>(entity =>
            {
                entity.Property(e => e.DestinationFieldName).HasMaxLength(128);

                entity.Property(e => e.SourceFieldName).HasMaxLength(128);

                entity.HasOne(d => d.OutsideImportMappingFunction)
                    .WithMany(p => p.OutsideImportMapping)
                    .HasForeignKey(d => d.OutsideImportMappingFunctionId)
                    .HasConstraintName("FK_AccountancyOutsiteMapping_AccountancyOutsiteMappingFunction");
            });

            modelBuilder.Entity<OutsideImportMappingFunction>(entity =>
            {
                entity.HasIndex(e => e.FunctionName)
                    .HasName("IX_AccountancyOutsiteMappingFunction")
                    .IsUnique();

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.DestinationDetailsPropertyName).HasMaxLength(128);

                entity.Property(e => e.FunctionName).HasMaxLength(128);

                entity.Property(e => e.MappingFunctionKey)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ObjectIdFieldName).HasMaxLength(128);

                entity.Property(e => e.SourceDetailsPropertyName).HasMaxLength(128);
            });

            modelBuilder.Entity<OutsideImportMappingObject>(entity =>
            {
                entity.HasKey(e => new { e.OutsideImportMappingFunctionId, e.SourceId, e.InputBillFId })
                    .HasName("PK_AccountancyOutsiteMappingObject");

                entity.Property(e => e.SourceId).HasMaxLength(128);

                entity.Property(e => e.InputBillFId).HasColumnName("InputBill_F_Id");

                entity.HasOne(d => d.OutsideImportMappingFunction)
                    .WithMany(p => p.OutsideImportMappingObject)
                    .HasForeignKey(d => d.OutsideImportMappingFunctionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccountancyOutsiteMappingObject_AccountancyOutsiteMappingFunction");
            });

            modelBuilder.Entity<PrintConfig>(entity =>
            {
                entity.Property(e => e.GenerateToString).HasComment("");

                entity.Property(e => e.PrintConfigName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<ProgramingFunction>(entity =>
            {
                entity.Property(e => e.FunctionBody).IsRequired();

                entity.Property(e => e.ProgramingFunctionName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
