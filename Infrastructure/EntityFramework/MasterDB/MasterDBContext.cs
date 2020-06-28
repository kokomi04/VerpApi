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
        public virtual DbSet<BarcodeGenerate> BarcodeGenerate { get; set; }
        public virtual DbSet<BarcodeStandard> BarcodeStandard { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<CustomGenCode> CustomGenCode { get; set; }
        public virtual DbSet<CustomerStatus> CustomerStatus { get; set; }
        public virtual DbSet<CustomerType> CustomerType { get; set; }
        public virtual DbSet<FileStatus> FileStatus { get; set; }
        public virtual DbSet<FileType> FileType { get; set; }
        public virtual DbSet<FunctionLevel> FunctionLevel { get; set; }
        public virtual DbSet<Gender> Gender { get; set; }
        public virtual DbSet<InventoryType> InventoryType { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Method> Method { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
        public virtual DbSet<ModuleGroup> ModuleGroup { get; set; }
        public virtual DbSet<ObjectCustomGenCodeMapping> ObjectCustomGenCodeMapping { get; set; }
        public virtual DbSet<ObjectGenCode> ObjectGenCode { get; set; }
        public virtual DbSet<ObjectProcessType> ObjectProcessType { get; set; }
        public virtual DbSet<ObjectType> ObjectType { get; set; }
        public virtual DbSet<PackageOperationType> PackageOperationType { get; set; }
        public virtual DbSet<PackageOption> PackageOption { get; set; }
        public virtual DbSet<PackageType> PackageType { get; set; }
        public virtual DbSet<PoProcessStatus> PoProcessStatus { get; set; }
        public virtual DbSet<ProductStatus> ProductStatus { get; set; }
        public virtual DbSet<ProgramingLang> ProgramingLang { get; set; }
        public virtual DbSet<ProgramingLevel> ProgramingLevel { get; set; }
        public virtual DbSet<PurchaseOrderStatus> PurchaseOrderStatus { get; set; }
        public virtual DbSet<PurchasingRequestStatus> PurchasingRequestStatus { get; set; }
        public virtual DbSet<PurchasingSuggestStatus> PurchasingSuggestStatus { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RoleDataPermission> RoleDataPermission { get; set; }
        public virtual DbSet<RolePermission> RolePermission { get; set; }
        public virtual DbSet<RoleStatus> RoleStatus { get; set; }
        public virtual DbSet<StockOutputRule> StockOutputRule { get; set; }
        public virtual DbSet<TimeType> TimeType { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<UnitStatus> UnitStatus { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserActivityLog> UserActivityLog { get; set; }
        public virtual DbSet<UserActivityLogChange> UserActivityLogChange { get; set; }
        public virtual DbSet<UserStatus> UserStatus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<CustomGenCode>(entity =>
            {
                entity.Property(e => e.CodeLength).HasDefaultValueSql("((5))");

                entity.Property(e => e.CreatedTime).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CustomGenCodeName).HasMaxLength(128);

                entity.Property(e => e.DateFormat)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastCode)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LastValue).HasDefaultValueSql("('')");

                entity.Property(e => e.Prefix)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.ResetDate).HasColumnType("datetime");

                entity.Property(e => e.Seperator)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Suffix)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.TempCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CustomerStatus>(entity =>
            {
                entity.Property(e => e.CustomerStatusId).ValueGeneratedNever();

                entity.Property(e => e.CustomerStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<CustomerType>(entity =>
            {
                entity.Property(e => e.CustomerTypeId).ValueGeneratedNever();

                entity.Property(e => e.CustomerTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<FileStatus>(entity =>
            {
                entity.Property(e => e.FileStatusId).ValueGeneratedNever();

                entity.Property(e => e.FileStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<FileType>(entity =>
            {
                entity.Property(e => e.FileTypeId).ValueGeneratedNever();

                entity.Property(e => e.FileTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<FunctionLevel>(entity =>
            {
                entity.Property(e => e.FunctionLevelId).ValueGeneratedNever();

                entity.Property(e => e.FunctionLevelName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Gender>(entity =>
            {
                entity.Property(e => e.GenderId).ValueGeneratedNever();

                entity.Property(e => e.GenderName)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<InventoryType>(entity =>
            {
                entity.Property(e => e.InventoryTypeId).ValueGeneratedNever();

                entity.Property(e => e.InventoryTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MenuName).HasMaxLength(50);

                entity.Property(e => e.Param).HasMaxLength(255);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Url).HasMaxLength(128);
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

            modelBuilder.Entity<ObjectCustomGenCodeMapping>(entity =>
            {
                entity.HasIndex(e => new { e.ObjectTypeId, e.ObjectId })
                    .HasName("UK_ObjectCustomGenCode")
                    .IsUnique();
            });

            modelBuilder.Entity<ObjectGenCode>(entity =>
            {
                entity.Property(e => e.CodeLength).HasDefaultValueSql("((5))");

                entity.Property(e => e.CreatedTime).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateFormat)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.LastCode)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LastValue).HasDefaultValueSql("('')");

                entity.Property(e => e.ObjectTypeName).HasMaxLength(128);

                entity.Property(e => e.Prefix)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.ResetDate).HasColumnType("datetime");

                entity.Property(e => e.Seperator)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Suffix)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ObjectProcessType>(entity =>
            {
                entity.Property(e => e.ObjectProcessTypeId).ValueGeneratedNever();

                entity.Property(e => e.ObjectProcessTypeDescription).HasMaxLength(512);

                entity.Property(e => e.ObjectProcessTypeName)
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

            modelBuilder.Entity<PackageOperationType>(entity =>
            {
                entity.Property(e => e.PackageOperationTypeId).ValueGeneratedNever();

                entity.Property(e => e.PackageOperationTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PackageOption>(entity =>
            {
                entity.Property(e => e.PackageOptionId).ValueGeneratedNever();

                entity.Property(e => e.PackageOptionName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PackageType>(entity =>
            {
                entity.Property(e => e.PackageTypeId).ValueGeneratedNever();

                entity.Property(e => e.PackageTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PoProcessStatus>(entity =>
            {
                entity.Property(e => e.PoProcessStatusId).ValueGeneratedNever();

                entity.Property(e => e.PoProcessStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ProductStatus>(entity =>
            {
                entity.Property(e => e.ProductStatusId).ValueGeneratedNever();

                entity.Property(e => e.ProductStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ProgramingLang>(entity =>
            {
                entity.HasKey(e => e.ProgramingLevelId);

                entity.Property(e => e.ProgramingLevelId).ValueGeneratedNever();

                entity.Property(e => e.ProgramingLevelName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ProgramingLevel>(entity =>
            {
                entity.Property(e => e.ProgramingLevelId).ValueGeneratedNever();

                entity.Property(e => e.ProgramingLevelName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PurchaseOrderStatus>(entity =>
            {
                entity.Property(e => e.PurchaseOrderStatusId).ValueGeneratedNever();

                entity.Property(e => e.PurchaseOrderStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PurchasingRequestStatus>(entity =>
            {
                entity.Property(e => e.PurchasingRequestStatusId).ValueGeneratedNever();

                entity.Property(e => e.PurchasingRequestStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PurchasingSuggestStatus>(entity =>
            {
                entity.Property(e => e.PurchasingSuggestStatusId).ValueGeneratedNever();

                entity.Property(e => e.PurchasingSuggestStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.ChildrenRoleIds).HasMaxLength(1024);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.RootPath)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.ParentRole)
                    .WithMany(p => p.InverseParentRole)
                    .HasForeignKey(d => d.ParentRoleId)
                    .HasConstraintName("FK_Role_Role");

                entity.HasOne(d => d.RoleStatus)
                    .WithMany(p => p.Role)
                    .HasForeignKey(d => d.RoleStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_RoleStatus");
            });

            modelBuilder.Entity<RoleDataPermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ObjectTypeId, e.ObjectId });

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleDataPermission)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoleDataPermission_Role");
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

                entity.Property(e => e.UnitStatusId).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<UnitStatus>(entity =>
            {
                entity.Property(e => e.UnitStatusId).ValueGeneratedNever();

                entity.Property(e => e.UnitStatusName)
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

                entity.Property(e => e.MessageTypeId).HasDefaultValueSql("((1))");
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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
