using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ActivityLogDB
{
    public partial class ActivityLogDBContext : DbContext
    {
        public ActivityLogDBContext()
        {
        }

        public ActivityLogDBContext(DbContextOptions<ActivityLogDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<Subscription> Subscription { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLog { get; set; }
        public virtual DbSet<UserActivityLogChange> UserActivityLogChange { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.Property(e => e.IpAddress).HasMaxLength(128);

                entity.Property(e => e.Message).HasMaxLength(512);

                entity.Property(e => e.MessageResourceFormatData).HasMaxLength(512);

                entity.Property(e => e.MessageResourceName).HasMaxLength(512);

                entity.Property(e => e.MessageTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.SubsidiaryId).HasComment("");
            });

            modelBuilder.Entity<UserActivityLogChange>(entity =>
            {
                entity.HasKey(e => e.UserActivityLogId);

                entity.Property(e => e.UserActivityLogId).ValueGeneratedNever();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
