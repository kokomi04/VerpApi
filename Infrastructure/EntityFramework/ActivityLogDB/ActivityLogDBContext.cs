using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ActivityLogDB;

public partial class ActivityLogDBContext : DbContext
{
    public ActivityLogDBContext(DbContextOptions<ActivityLogDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notification { get; set; }

    public virtual DbSet<PushSubscription> PushSubscription { get; set; }

    public virtual DbSet<Subscription> Subscription { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLog { get; set; }

    public virtual DbSet<UserActivityLogChange> UserActivityLogChange { get; set; }

    public virtual DbSet<UserLoginLog> UserLoginLog { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.HasKey(e => e.PushSubscriptionId).HasName("PK_PushSubcription");

            entity.Property(e => e.Auth)
                .IsRequired()
                .HasMaxLength(2048);
            entity.Property(e => e.Endpoint)
                .IsRequired()
                .HasMaxLength(2048);
            entity.Property(e => e.P256dh)
                .IsRequired()
                .HasMaxLength(2048);
        });

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

        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.Property(e => e.IpAddress).HasMaxLength(128);
            entity.Property(e => e.Message).HasMaxLength(512);
            entity.Property(e => e.MessageResourceFormatData).HasMaxLength(512);
            entity.Property(e => e.MessageResourceName).HasMaxLength(512);
            entity.Property(e => e.MessageTypeId).HasDefaultValueSql("((1))");
            entity.Property(e => e.StrSubId).HasMaxLength(128);
            entity.Property(e => e.UserName).HasMaxLength(128);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
