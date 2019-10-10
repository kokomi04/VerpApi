using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class MasterDBContext : DbContext
    {
        public MasterDBContext()
        {
        }
        public MasterDBContext(DbContextOptions<MasterDBContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Action> Action { get; set; }
        public virtual DbSet<ApiEndpoint> ApiEndpoint { get; set; }
        public virtual DbSet<BarcodeConfig> BarcodeConfig { get; set; }
        public virtual DbSet<BarcodeStandard> BarcodeStandard { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Gender> Gender { get; set; }
        public virtual DbSet<Method> Method { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
        public virtual DbSet<ModuleGroup> ModuleGroup { get; set; }
        public virtual DbSet<ObjectType> ObjectType { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RolePermission> RolePermission { get; set; }
        public virtual DbSet<RoleStatus> RoleStatus { get; set; }
        public virtual DbSet<StockOutputRule> StockOutputRule { get; set; }
        public virtual DbSet<TimeType> TimeType { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLog { get; set; }
        public virtual DbSet<UserActivityLogChange> UserActivityLogChange { get; set; }
        public virtual DbSet<UserStatus> UserStatus { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }
        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<Action>(entity =>
            {
                entity.Property(e => e.ActionId).ValueGeneratedNever();
                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(16);
            });
            modelBuilder.Entity<ApiEndpoint>(entity =>
            {
                entity.Property(e => e.ApiEndpointId).ValueGeneratedNever();
                entity.Property(e => e.Route)
                    .IsRequired()
                    .HasMaxLength(512);
                entity.HasOne(d => d.Action)
                    .WithMany(p => p.ApiEndpoint)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ApiEndpoint_Action");
                entity.HasOne(d => d.Method)
                    .WithMany(p => p.ApiEndpoint)
                    .HasForeignKey(d => d.MethodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ApiEndpoint_Method");
            });
            modelBuilder.Entity<BarcodeConfig>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
            });
            modelBuilder.Entity<BarcodeStandard>(entity =>
            {
                entity.Property(e => e.BarcodeStandardId).ValueGeneratedNever();
                entity.Property(e => e.BarcodeStandardName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<Config>(entity =>
            {
                entity.Property(e => e.ConfigId).ValueGeneratedNever();
                entity.Property(e => e.ConfigName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);
            });
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).ValueGeneratedNever();
                entity.Property(e => e.Address).HasMaxLength(512);
                entity.Property(e => e.Email).HasMaxLength(128);
                entity.Property(e => e.EmployeeCode).HasMaxLength(64);
                entity.Property(e => e.FullName).HasMaxLength(128);
                entity.Property(e => e.Phone).HasMaxLength(64);
                entity.HasOne(d => d.Gender)
                    .WithMany(p => p.Employee)
                    .HasForeignKey(d => d.GenderId)
                    .HasConstraintName("FK_Employee_Gender");
            });
            modelBuilder.Entity<Gender>(entity =>
            {
                entity.Property(e => e.GenderId).ValueGeneratedNever();
                entity.Property(e => e.GenderName)
                    .IsRequired()
                    .HasMaxLength(64);
            });
            modelBuilder.Entity<Method>(entity =>
            {
                entity.Property(e => e.MethodId).ValueGeneratedNever();
                entity.Property(e => e.MethodName)
                    .IsRequired()
                    .HasMaxLength(16);
            });
            modelBuilder.Entity<Module>(entity =>
            {
                entity.Property(e => e.ModuleId).ValueGeneratedNever();
                entity.Property(e => e.Description).HasMaxLength(512);
                entity.Property(e => e.ModuleName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.ModuleGroup)
                    .WithMany(p => p.Module)
                    .HasForeignKey(d => d.ModuleGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Module_ModuleGroup");
            });
            modelBuilder.Entity<ModuleApiEndpointMapping>(entity =>
            {
                entity.HasKey(e => new { e.ModuleId, e.ApiEndpointId });
                entity.HasOne(d => d.ApiEndpoint)
                    .WithMany(p => p.ModuleApiEndpointMapping)
                    .HasForeignKey(d => d.ApiEndpointId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModuleApiEndpointMapping_ApiEndpoint");
                entity.HasOne(d => d.Module)
                    .WithMany(p => p.ModuleApiEndpointMapping)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModuleApiEndpointMapping_Module");
            });
            modelBuilder.Entity<ModuleGroup>(entity =>
            {
                entity.Property(e => e.ModuleGroupId).ValueGeneratedNever();
                entity.Property(e => e.ModuleGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<ObjectType>(entity =>
            {
                entity.Property(e => e.ObjectTypeId).ValueGeneratedNever();
                entity.Property(e => e.ObjectTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Description).HasMaxLength(512);
                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
                entity.HasOne(d => d.RoleStatus)
                    .WithMany(p => p.Role)
                    .HasForeignKey(d => d.RoleStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_RoleStatus");
            });
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ModuleId });
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
                entity.HasOne(d => d.Module)
                    .WithMany(p => p.RolePermission)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePermission_Module");
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RolePermission)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RolePermission_Role");
            });
            modelBuilder.Entity<RoleStatus>(entity =>
            {
                entity.Property(e => e.RoleStatusId).ValueGeneratedNever();
                entity.Property(e => e.RoleStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<StockOutputRule>(entity =>
            {
                entity.Property(e => e.StockOutputRuleId).ValueGeneratedNever();
                entity.Property(e => e.StockOutputRuleName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<TimeType>(entity =>
            {
                entity.Property(e => e.TimeTypeId).ValueGeneratedNever();
                entity.Property(e => e.TimeTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.UnitName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(64);
            });
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.Property(e => e.Message).HasMaxLength(512);
            });
            modelBuilder.Entity<UserActivityLogChange>(entity =>
            {
                entity.HasKey(e => e.UserActivityLogId);
                entity.Property(e => e.UserActivityLogId).ValueGeneratedNever();
            });
            modelBuilder.Entity<UserStatus>(entity =>
            {
                entity.Property(e => e.UserStatusId).ValueGeneratedNever();
                entity.Property(e => e.UserStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
        }
    }
}
