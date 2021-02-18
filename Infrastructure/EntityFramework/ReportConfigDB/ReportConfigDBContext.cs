using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class ReportConfigDBContext : DbContext
    {
        public ReportConfigDBContext()
        {
        }

        public ReportConfigDBContext(DbContextOptions<ReportConfigDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ReportType> ReportType { get; set; }
        public virtual DbSet<ReportTypeGroup> ReportTypeGroup { get; set; }
        public virtual DbSet<ReportTypeView> ReportTypeView { get; set; }
        public virtual DbSet<ReportTypeViewField> ReportTypeViewField { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

                entity.HasOne(d => d.ReportTypeGroup)
                    .WithMany(p => p.ReportType)
                    .HasForeignKey(d => d.ReportTypeGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReportType_ReportGroup");
            });

            modelBuilder.Entity<ReportTypeGroup>(entity =>
            {
                entity.Property(e => e.ReportTypeGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ReportTypeView>(entity =>
            {
                entity.Property(e => e.ReportTypeViewName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.ReportType)
                    .WithMany(p => p.ReportTypeView)
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

                entity.HasOne(d => d.ReportTypeView)
                    .WithMany(p => p.ReportTypeViewField)
                    .HasForeignKey(d => d.ReportTypeViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReportTypeViewField_ReportTypeView");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
