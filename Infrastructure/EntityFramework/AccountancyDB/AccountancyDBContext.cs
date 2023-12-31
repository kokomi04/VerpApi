using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VErp.Infrastructure.EF.AccountancyDB;

public partial class AccountancyDBContext : DbContext
{
    public AccountancyDBContext(DbContextOptions<AccountancyDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CalcPeriod> CalcPeriod { get; set; }

    public virtual DbSet<InputArea> InputArea { get; set; }

    public virtual DbSet<InputAreaField> InputAreaField { get; set; }

    public virtual DbSet<InputBill> InputBill { get; set; }

    public virtual DbSet<InputBillAllocation> InputBillAllocation { get; set; }

    public virtual DbSet<InputField> InputField { get; set; }

    public virtual DbSet<InputType> InputType { get; set; }

    public virtual DbSet<InputTypeGlobalSetting> InputTypeGlobalSetting { get; set; }

    public virtual DbSet<InputTypeGroup> InputTypeGroup { get; set; }

    public virtual DbSet<InputTypeView> InputTypeView { get; set; }

    public virtual DbSet<InputTypeViewField> InputTypeViewField { get; set; }

    public virtual DbSet<ProgramingFunction> ProgramingFunction { get; set; }

    public virtual DbSet<RefObjectApprovalStep> RefObjectApprovalStep { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalcPeriod>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Title).HasMaxLength(512);
        });

        modelBuilder.Entity<InputArea>(entity =>
        {
            entity.Property(e => e.Columns).HasDefaultValueSql("((1))");
            entity.Property(e => e.InputAreaCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.InputType).WithMany(p => p.InputArea)
                .HasForeignKey(d => d.InputTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputArea_InputType");
        });

        modelBuilder.Entity<InputAreaField>(entity =>
        {
            entity.HasKey(e => e.InputAreaFieldId).HasName("PK__InputAre__C2457D60A15F171E");

            entity.HasIndex(e => new { e.InputTypeId, e.InputFieldId }, "IX_InputAreaField").IsUnique();

            entity.Property(e => e.Column).HasDefaultValueSql("((1))");
            entity.Property(e => e.CustomButtonHtml).HasMaxLength(128);
            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.FiltersName).HasMaxLength(128);
            entity.Property(e => e.InputStyleJson).HasMaxLength(512);
            entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.ReferenceUrl).HasMaxLength(1024);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.RequireFiltersName).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);
            entity.Property(e => e.TitleStyleJson).HasMaxLength(512);

            entity.HasOne(d => d.InputArea).WithMany(p => p.InputAreaField)
                .HasForeignKey(d => d.InputAreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputAreaField_InputArea");

            entity.HasOne(d => d.InputField).WithMany(p => p.InputAreaField)
                .HasForeignKey(d => d.InputFieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputAreaField_InputField");

            entity.HasOne(d => d.InputType).WithMany(p => p.InputAreaField)
                .HasForeignKey(d => d.InputTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputAreaField_InputType");
        });

        modelBuilder.Entity<InputBill>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("PK_InputValueBill");

            entity.HasIndex(e => new { e.SubsidiaryId, e.BillCode }, "IX_InputBill_BillCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.FId).HasColumnName("F_Id");
            entity.Property(e => e.BillCode).HasMaxLength(512);
            entity.Property(e => e.ParentInputBillFId).HasColumnName("ParentInputBill_F_Id");

            entity.HasOne(d => d.InputType).WithMany(p => p.InputBill)
                .HasForeignKey(d => d.InputTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputValueBill_InputType");

            entity.HasOne(d => d.ParentInputBillF).WithMany(p => p.InverseParentInputBillF)
                .HasForeignKey(d => d.ParentInputBillFId)
                .HasConstraintName("FK_InputBill_InputBill");
        });

        modelBuilder.Entity<InputBillAllocation>(entity =>
        {
            entity.HasKey(e => new { e.ParentInputBillFId, e.DataAllowcationBillCode });

            entity.Property(e => e.ParentInputBillFId).HasColumnName("Parent_InputBill_F_Id");
            entity.Property(e => e.DataAllowcationBillCode)
                .HasMaxLength(128)
                .HasColumnName("DataAllowcation_BillCode");

            entity.HasOne(d => d.ParentInputBillF).WithMany(p => p.InputBillAllocation)
                .HasForeignKey(d => d.ParentInputBillFId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputBillAllocation_InputBill");
        });

        modelBuilder.Entity<InputField>(entity =>
        {
            entity.Property(e => e.CustomButtonHtml).HasMaxLength(128);
            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.RefTableCode).HasMaxLength(128);
            entity.Property(e => e.RefTableField).HasMaxLength(128);
            entity.Property(e => e.RefTableTitle).HasMaxLength(512);
            entity.Property(e => e.ReferenceUrl).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<InputType>(entity =>
        {
            entity.Property(e => e.DataAllowcationInputTypeIds).HasMaxLength(1024);
            entity.Property(e => e.InputTypeCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.InputTypeGroup).WithMany(p => p.InputType)
                .HasForeignKey(d => d.InputTypeGroupId)
                .HasConstraintName("FK_InputType_InputTypeGroup");

            entity.HasOne(d => d.ResultAllowcationInputType).WithMany(p => p.InverseResultAllowcationInputType)
                .HasForeignKey(d => d.ResultAllowcationInputTypeId)
                .HasConstraintName("FK_InputType_InputType");
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

            entity.HasOne(d => d.InputType).WithMany(p => p.InputTypeView)
                .HasForeignKey(d => d.InputTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputTypeView_InputType");
        });

        modelBuilder.Entity<InputTypeViewField>(entity =>
        {
            entity.HasKey(e => e.InputTypeViewFieldId).HasName("PK_InputTypeViewField_1");

            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.RefTableCode).HasMaxLength(128);
            entity.Property(e => e.RefTableField).HasMaxLength(128);
            entity.Property(e => e.RefTableTitle).HasMaxLength(512);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.SelectFilters).HasMaxLength(512);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.InputTypeView).WithMany(p => p.InputTypeViewField)
                .HasForeignKey(d => d.InputTypeViewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InputTypeViewField_InputTypeView");
        });

        modelBuilder.Entity<ProgramingFunction>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.FunctionBody).IsRequired();
            entity.Property(e => e.ProgramingFunctionName)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<RefObjectApprovalStep>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("RefObjectApprovalStep");

            entity.Property(e => e.ObjectApprovalStepId).ValueGeneratedOnAdd();
            entity.Property(e => e.ObjectFieldEnable).HasMaxLength(1024);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
