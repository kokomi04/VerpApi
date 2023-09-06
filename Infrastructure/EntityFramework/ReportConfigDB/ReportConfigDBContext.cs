using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VErp.Infrastructure.EF.ReportConfigDB;

public partial class ReportConfigDBContext : DbContext
{
    public ReportConfigDBContext(DbContextOptions<ReportConfigDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DashboardType> DashboardType { get; set; }

    public virtual DbSet<DashboardTypeGroup> DashboardTypeGroup { get; set; }

    public virtual DbSet<DashboardTypeView> DashboardTypeView { get; set; }

    public virtual DbSet<DashboardTypeViewField> DashboardTypeViewField { get; set; }

    public virtual DbSet<ReportType> ReportType { get; set; }

    public virtual DbSet<ReportTypeCustom> ReportTypeCustom { get; set; }

    public virtual DbSet<ReportTypeGroup> ReportTypeGroup { get; set; }

    public virtual DbSet<ReportTypeView> ReportTypeView { get; set; }

    public virtual DbSet<ReportTypeViewField> ReportTypeViewField { get; set; }

    public virtual DbSet<ReportTypeViewFieldValue> ReportTypeViewFieldValue { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DashboardType>(entity =>
        {
            entity.Property(e => e.DashboardTypeName)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasOne(d => d.DashboardTypeGroup).WithMany(p => p.DashboardType)
                .HasForeignKey(d => d.DashboardTypeGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DashboardType_DashboardTypeGroup");
        });

        modelBuilder.Entity<DashboardTypeGroup>(entity =>
        {
            entity.Property(e => e.DashboardTypeGroupName)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<DashboardTypeView>(entity =>
        {
            entity.Property(e => e.DashboardTypeViewName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.DashboardType).WithMany(p => p.DashboardTypeView)
                .HasForeignKey(d => d.DashboardTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DashboardTypeView_DashboardType");
        });

        modelBuilder.Entity<DashboardTypeViewField>(entity =>
        {
            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.HelpText).HasMaxLength(512);
            entity.Property(e => e.ParamerterName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.RefFilters).HasMaxLength(512);
            entity.Property(e => e.RefTableCode).HasMaxLength(128);
            entity.Property(e => e.RefTableField).HasMaxLength(128);
            entity.Property(e => e.RefTableTitle).HasMaxLength(512);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.DashboardTypeView).WithMany(p => p.DashboardTypeViewField)
                .HasForeignKey(d => d.DashboardTypeViewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DashboardTypeViewField_DashboardTypeView");
        });

        modelBuilder.Entity<ReportType>(entity =>
        {
            entity.Property(e => e.MainView).HasMaxLength(128);
            entity.Property(e => e.PrintTitle).HasMaxLength(128);
            entity.Property(e => e.ReportPath)
                .IsRequired()
                .HasMaxLength(512);
            entity.Property(e => e.ReportTypeName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.TemplateFileId).HasComment("");

            entity.HasOne(d => d.ReportTypeGroup).WithMany(p => p.ReportType)
                .HasForeignKey(d => d.ReportTypeGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportType_ReportGroup");
        });

        modelBuilder.Entity<ReportTypeCustom>(entity =>
        {
            entity.HasKey(e => e.ReportTypeCustomId).HasName("PK__ReportTy__8A5B8027D56A757C");
        });

        modelBuilder.Entity<ReportTypeGroup>(entity =>
        {
            entity.HasKey(e => e.ReportTypeGroupId).HasName("PK_ReportGroup");

            entity.Property(e => e.ReportTypeGroupName)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<ReportTypeView>(entity =>
        {
            entity.Property(e => e.ReportTypeViewName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.ReportType).WithMany(p => p.ReportTypeView)
                .HasForeignKey(d => d.ReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportTypeView_ReportType");
        });

        modelBuilder.Entity<ReportTypeViewField>(entity =>
        {
            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.HelpText).HasMaxLength(512);
            entity.Property(e => e.ParamerterName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.RefFilters).HasMaxLength(512);
            entity.Property(e => e.RefTableCode).HasMaxLength(128);
            entity.Property(e => e.RefTableField).HasMaxLength(128);
            entity.Property(e => e.RefTableTitle).HasMaxLength(512);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.ReportTypeView).WithMany(p => p.ReportTypeViewField)
                .HasForeignKey(d => d.ReportTypeViewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportTypeViewField_ReportTypeView");
        });

        modelBuilder.Entity<ReportTypeViewFieldValue>(entity =>
        {
            entity.HasKey(e => new { e.ReportTypeViewFieldId, e.SubsidiaryId });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
