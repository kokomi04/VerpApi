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
        public virtual DbSet<ActionType> ActionType { get; set; }
        public virtual DbSet<ApiEndpoint> ApiEndpoint { get; set; }
        public virtual DbSet<BackupStorage> BackupStorage { get; set; }
        public virtual DbSet<BarcodeConfig> BarcodeConfig { get; set; }
        public virtual DbSet<BarcodeGenerate> BarcodeGenerate { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryField> CategoryField { get; set; }
        public virtual DbSet<CategoryGroup> CategoryGroup { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<CustomGenCode> CustomGenCode { get; set; }
        public virtual DbSet<CustomGenCodeValue> CustomGenCodeValue { get; set; }
        public virtual DbSet<DataConfig> DataConfig { get; set; }
        public virtual DbSet<Guide> Guide { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Method> Method { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
        public virtual DbSet<ModuleGroup> ModuleGroup { get; set; }
        public virtual DbSet<ObjectCustomGenCodeMapping> ObjectCustomGenCodeMapping { get; set; }
        public virtual DbSet<OutSideDataConfig> OutSideDataConfig { get; set; }
        public virtual DbSet<OutsideDataFieldConfig> OutsideDataFieldConfig { get; set; }
        public virtual DbSet<OutsideImportMapping> OutsideImportMapping { get; set; }
        public virtual DbSet<OutsideImportMappingFunction> OutsideImportMappingFunction { get; set; }
        public virtual DbSet<OutsideImportMappingObject> OutsideImportMappingObject { get; set; }
        public virtual DbSet<PrintConfig> PrintConfig { get; set; }
        public virtual DbSet<PrintConfigDetail> PrintConfigDetail { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RoleDataPermission> RoleDataPermission { get; set; }
        public virtual DbSet<RolePermission> RolePermission { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<User> User { get; set; }

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

            modelBuilder.Entity<ActionType>(entity =>
            {
                entity.Property(e => e.ActionTypeId).ValueGeneratedNever();

                entity.Property(e => e.ActionTitle)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.ActionTypeName)
                    .IsRequired()
                    .HasMaxLength(64);
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

            modelBuilder.Entity<BackupStorage>(entity =>
            {
                entity.HasKey(e => new { e.ModuleTypeId, e.BackupPoint });

                entity.Property(e => e.BackupDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedByUserId).HasComment("");

                entity.Property(e => e.FileId).HasComment("");

                entity.Property(e => e.RestoreDate).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedByUserId).HasComment("");
            });

            modelBuilder.Entity<BarcodeConfig>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryCode)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(256);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.UsePlace)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.CategoryGroup)
                    .WithMany(p => p.Category)
                    .HasForeignKey(d => d.CategoryGroupId)
                    .HasConstraintName("FK_Category_CategoryGroup");
            });

            modelBuilder.Entity<CategoryField>(entity =>
            {
                entity.Property(e => e.CategoryFieldName)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.RefTableCode)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefTableField)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefTableTitle)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RegularExpression)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryField)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryField_Category");
            });

            modelBuilder.Entity<CategoryGroup>(entity =>
            {
                entity.Property(e => e.CategoryGroupName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
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
                entity.Property(e => e.BaseFormat).HasMaxLength(128);

                entity.Property(e => e.CodeFormat).HasMaxLength(128);

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

            modelBuilder.Entity<CustomGenCodeValue>(entity =>
            {
                entity.HasKey(e => new { e.CustomGenCodeId, e.BaseValue });

                entity.Property(e => e.BaseValue).HasMaxLength(128);

                entity.Property(e => e.LastCode)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LastValue).HasDefaultValueSql("('')");

                entity.Property(e => e.TempCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DataConfig>(entity =>
            {
                entity.HasKey(e => e.SubsidiaryId);

                entity.Property(e => e.SubsidiaryId).ValueGeneratedNever();

                entity.Property(e => e.AutoClosingDate).HasComment("");

                entity.Property(e => e.ClosingDate).HasColumnType("datetime");

                entity.Property(e => e.FreqClosingDate)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('{}')")
                    .HasComment("");
            });

            modelBuilder.Entity<Guide>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.GuideCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
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

                entity.Property(e => e.IsDeveloper).HasComment("");

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

            modelBuilder.Entity<OutSideDataConfig>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PK__OutSideD__19093A0B3AED9766");

                entity.Property(e => e.CategoryId).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Key)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ParentKey)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithOne(p => p.OutSideDataConfig)
                    .HasForeignKey<OutSideDataConfig>(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutSideDataConfig_Category");
            });

            modelBuilder.Entity<OutsideDataFieldConfig>(entity =>
            {
                entity.Property(e => e.Alias).HasMaxLength(512);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.HasOne(d => d.OutsideDataConfig)
                    .WithMany(p => p.OutsideDataFieldConfig)
                    .HasForeignKey(d => d.OutsideDataConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OutsideDataFieldConfig_OutSideDataConfig");
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
                entity.Property(e => e.PrintConfigName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<PrintConfigDetail>(entity =>
            {
                entity.HasOne(d => d.PrintConfig)
                    .WithMany(p => p.PrintConfigDetail)
                    .HasForeignKey(d => d.PrintConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrintConfigDetail_PrintConfig");
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
                entity.HasKey(e => new { e.RoleId, e.ModuleId, e.ObjectTypeId, e.ObjectId });

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.JsonActionIds).HasMaxLength(512);

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

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.UnitName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UnitStatusId).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.AccessFailedCount).HasComment("");

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
