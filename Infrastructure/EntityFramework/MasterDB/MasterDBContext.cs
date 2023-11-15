using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class MasterDBContext : DbContext
{
    public MasterDBContext(DbContextOptions<MasterDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Action { get; set; }

    public virtual DbSet<ActionButton> ActionButton { get; set; }

    public virtual DbSet<ActionButtonBillType> ActionButtonBillType { get; set; }

    public virtual DbSet<ActionType> ActionType { get; set; }

    public virtual DbSet<ApiEndpoint> ApiEndpoint { get; set; }

    public virtual DbSet<BackupStorage> BackupStorage { get; set; }

    public virtual DbSet<BarcodeConfig> BarcodeConfig { get; set; }

    public virtual DbSet<BarcodeGenerate> BarcodeGenerate { get; set; }

    public virtual DbSet<Category> Category { get; set; }

    public virtual DbSet<CategoryField> CategoryField { get; set; }

    public virtual DbSet<CategoryGroup> CategoryGroup { get; set; }

    public virtual DbSet<CategoryView> CategoryView { get; set; }

    public virtual DbSet<CategoryViewField> CategoryViewField { get; set; }

    public virtual DbSet<Config> Config { get; set; }

    public virtual DbSet<CurrencyConvert> CurrencyConvert { get; set; }

    public virtual DbSet<CustomGenCode> CustomGenCode { get; set; }

    public virtual DbSet<CustomGenCodeValue> CustomGenCodeValue { get; set; }

    public virtual DbSet<DataConfig> DataConfig { get; set; }

    public virtual DbSet<EmailConfiguration> EmailConfiguration { get; set; }

    public virtual DbSet<FileConfiguration> FileConfiguration { get; set; }

    public virtual DbSet<Guide> Guide { get; set; }

    public virtual DbSet<GuideCate> GuideCate { get; set; }

    public virtual DbSet<I18nLanguage> I18nLanguage { get; set; }

    public virtual DbSet<MailTemplate> MailTemplate { get; set; }

    public virtual DbSet<Menu> Menu { get; set; }

    public virtual DbSet<Method> Method { get; set; }

    public virtual DbSet<Module> Module { get; set; }

    public virtual DbSet<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }

    public virtual DbSet<ModuleGroup> ModuleGroup { get; set; }

    public virtual DbSet<Notification> Notification { get; set; }

    public virtual DbSet<ObjectCustomGenCodeMapping> ObjectCustomGenCodeMapping { get; set; }

    public virtual DbSet<ObjectPrintConfigMapping> ObjectPrintConfigMapping { get; set; }

    public virtual DbSet<ObjectPrintConfigStandardMapping> ObjectPrintConfigStandardMapping { get; set; }

    public virtual DbSet<OutsideImportMapping> OutsideImportMapping { get; set; }

    public virtual DbSet<OutsideImportMappingFunction> OutsideImportMappingFunction { get; set; }

    public virtual DbSet<OutsideImportMappingObject> OutsideImportMappingObject { get; set; }

    public virtual DbSet<PrintConfigCustom> PrintConfigCustom { get; set; }

    public virtual DbSet<PrintConfigCustomModuleType> PrintConfigCustomModuleType { get; set; }

    public virtual DbSet<PrintConfigHeaderCustom> PrintConfigHeaderCustom { get; set; }

    public virtual DbSet<PrintConfigHeaderStandard> PrintConfigHeaderStandard { get; set; }

    public virtual DbSet<PrintConfigStandard> PrintConfigStandard { get; set; }

    public virtual DbSet<PrintConfigStandardModuleType> PrintConfigStandardModuleType { get; set; }

    public virtual DbSet<ReuseContent> ReuseContent { get; set; }

    public virtual DbSet<Role> Role { get; set; }

    public virtual DbSet<RoleDataPermission> RoleDataPermission { get; set; }

    public virtual DbSet<RolePermission> RolePermission { get; set; }

    public virtual DbSet<Subscription> Subscription { get; set; }

    public virtual DbSet<Unit> Unit { get; set; }

    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<UserProgramingFunction> UserProgramingFunction { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.Property(e => e.ActionId).ValueGeneratedNever();
            entity.Property(e => e.ActionName)
                .IsRequired()
                .HasMaxLength(16);
        });

        modelBuilder.Entity<ActionButton>(entity =>
        {
            entity.Property(e => e.ActionButtonCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ActionPositionId).HasDefaultValueSql("((2))");
            entity.Property(e => e.IconName).HasMaxLength(25);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<ActionButtonBillType>(entity =>
        {
            entity.HasKey(e => new { e.ActionButtonId, e.BillTypeObjectId });

            entity.HasOne(d => d.ActionButton).WithMany(p => p.ActionButtonBillType)
                .HasForeignKey(d => d.ActionButtonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActionButtonBillType_ActionButton");
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

            entity.HasOne(d => d.Action).WithMany(p => p.ApiEndpoint)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApiEndpoint_Action");

            entity.HasOne(d => d.Method).WithMany(p => p.ApiEndpoint)
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
            entity.HasKey(e => e.BarcodeConfigId).HasName("PK_BarcodeStandardConfig");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B39EED0A9");

            entity.HasIndex(e => e.CategoryCode, "IX_Category_CategoryCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CategoryCode)
                .IsRequired()
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.MenuId).HasComment("");
            entity.Property(e => e.ParentKey)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ParentTitle).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.UsePlace)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.CategoryGroup).WithMany(p => p.Category)
                .HasForeignKey(d => d.CategoryGroupId)
                .HasConstraintName("FK_Category_CategoryGroup");
        });

        modelBuilder.Entity<CategoryField>(entity =>
        {
            entity.HasKey(e => e.CategoryFieldId).HasName("PK__Category__213B0412188B59DC");

            entity.HasIndex(e => e.CategoryId, "IDX_CategoryId");

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

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryField)
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

        modelBuilder.Entity<CategoryView>(entity =>
        {
            entity.HasKey(e => e.CategoryViewId).HasName("PK_ReportTypeView");

            entity.Property(e => e.CategoryViewName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryView)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FK_ReportTypeView_ReportTypeView_Category");
        });

        modelBuilder.Entity<CategoryViewField>(entity =>
        {
            entity.HasKey(e => e.CategoryViewFieldId).HasName("PK_ReportTypeViewField");

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

            entity.HasOne(d => d.CategoryView).WithMany(p => p.CategoryViewField)
                .HasForeignKey(d => d.CategoryViewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryViewField_CategoryView");
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

        modelBuilder.Entity<CurrencyConvert>(entity =>
        {
            entity.Property(e => e.FromCurrency)
                .IsRequired()
                .HasMaxLength(16);
            entity.Property(e => e.Rate).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.ToCurrency)
                .IsRequired()
                .HasMaxLength(16);
        });

        modelBuilder.Entity<CustomGenCode>(entity =>
        {
            entity.HasKey(e => e.CustomGenCodeId).HasName("PK_ObjectGenCode_copy1");

            entity.Property(e => e.BaseFormat).HasMaxLength(128);
            entity.Property(e => e.CodeFormat).HasMaxLength(128);
            entity.Property(e => e.CodeLength).HasDefaultValueSql("((5))");
            entity.Property(e => e.CreatedDatetimeUtc)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedTime).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CustomGenCodeName).HasMaxLength(128);
            entity.Property(e => e.DeletedDatetimeUtc).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.LastCode)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");
            entity.Property(e => e.ResetDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDatetimeUtc)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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
            entity.Property(e => e.WorkingFromDate).HasColumnType("datetime");
            entity.Property(e => e.WorkingToDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EmailConfiguration>(entity =>
        {
            entity.Property(e => e.IsSsl)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MailFrom)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.SmtpHost)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<Guide>(entity =>
        {
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.GuideCode)
                .IsRequired()
                .HasMaxLength(128)
                .HasComment("");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<GuideCate>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Title).HasMaxLength(512);
        });

        modelBuilder.Entity<I18nLanguage>(entity =>
        {
            entity.Property(e => e.En).HasMaxLength(1024);
            entity.Property(e => e.Key)
                .HasMaxLength(1024)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS");
            entity.Property(e => e.Vi).HasMaxLength(1024);
        });

        modelBuilder.Entity<MailTemplate>(entity =>
        {
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.TemplateCode)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED230B7ACE4CD");

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

            entity.HasOne(d => d.ModuleGroup).WithMany(p => p.Module)
                .HasForeignKey(d => d.ModuleGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Module_ModuleGroup");
        });

        modelBuilder.Entity<ModuleApiEndpointMapping>(entity =>
        {
            entity.HasIndex(e => new { e.ModuleId, e.ApiEndpointId }, "IX_ModuleApiEndpointMapping").IsUnique();

            entity.HasOne(d => d.ApiEndpoint).WithMany(p => p.ModuleApiEndpointMapping)
                .HasForeignKey(d => d.ApiEndpointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModuleApiEndpointMapping_ApiEndpoint");

            entity.HasOne(d => d.Module).WithMany(p => p.ModuleApiEndpointMapping)
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
            entity.HasKey(e => e.ObjectCustomGenCodeMappingId).HasName("PK__ObjectCu__3214EC0706DC7356");
        });

        modelBuilder.Entity<ObjectPrintConfigMapping>(entity =>
        {
            entity.HasKey(e => new { e.PrintConfigCustomId, e.ObjectTypeId, e.ObjectId }).HasName("PK_ObjectPrintConfigMapping_1");

            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<ObjectPrintConfigStandardMapping>(entity =>
        {
            entity.HasKey(e => new { e.ObjectId, e.ObjectTypeId, e.PrintConfigStandardId }).HasName("PK__ObjectPr__33C2DE8ADACC08E6");

            entity.Property(e => e.UpdatedDatetimeUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<OutsideImportMapping>(entity =>
        {
            entity.HasKey(e => e.OutsideImportMappingId).HasName("PK_AccountancyOutsiteMapping");

            entity.Property(e => e.DestinationFieldName).HasMaxLength(128);
            entity.Property(e => e.SourceFieldName).HasMaxLength(128);

            entity.HasOne(d => d.OutsideImportMappingFunction).WithMany(p => p.OutsideImportMapping)
                .HasForeignKey(d => d.OutsideImportMappingFunctionId)
                .HasConstraintName("FK_AccountancyOutsiteMapping_AccountancyOutsiteMappingFunction");
        });

        modelBuilder.Entity<OutsideImportMappingFunction>(entity =>
        {
            entity.HasKey(e => e.OutsideImportMappingFunctionId).HasName("PK_AccountancyOutsiteMappingFunction");

            entity.HasIndex(e => e.FunctionName, "IX_AccountancyOutsiteMappingFunction").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.DestinationDetailsPropertyName).HasMaxLength(128);
            entity.Property(e => e.FunctionName).HasMaxLength(128);
            entity.Property(e => e.MappingFunctionKey)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.ObjectIdFieldName).HasMaxLength(128);
            entity.Property(e => e.ObjectTypeId).HasDefaultValueSql("((39))");
            entity.Property(e => e.SourceDetailsPropertyName).HasMaxLength(128);
        });

        modelBuilder.Entity<OutsideImportMappingObject>(entity =>
        {
            entity.HasKey(e => new { e.OutsideImportMappingFunctionId, e.SourceId, e.InputBillFId }).HasName("PK_AccountancyOutsiteMappingObject");

            entity.Property(e => e.SourceId).HasMaxLength(128);
            entity.Property(e => e.InputBillFId).HasColumnName("InputBill_F_Id");
            entity.Property(e => e.BillObjectTypeId).HasDefaultValueSql("((39))");

            entity.HasOne(d => d.OutsideImportMappingFunction).WithMany(p => p.OutsideImportMappingObject)
                .HasForeignKey(d => d.OutsideImportMappingFunctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountancyOutsiteMappingObject_AccountancyOutsiteMappingFunction");
        });

        modelBuilder.Entity<PrintConfigCustom>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.Property(e => e.PrintConfigName).HasMaxLength(255);
            entity.Property(e => e.TemplateFileName).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<PrintConfigCustomModuleType>(entity =>
        {
            entity.HasKey(e => new { e.PrintConfigCustomId, e.ModuleTypeId });

            entity.HasOne(d => d.PrintConfigCustom).WithMany(p => p.PrintConfigCustomModuleType)
                .HasForeignKey(d => d.PrintConfigCustomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintConfigCustomModuleType_PrintConfigCustom");
        });

        modelBuilder.Entity<PrintConfigHeaderCustom>(entity =>
        {
            entity.Property(e => e.PrintConfigHeaderCustomCode).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<PrintConfigHeaderStandard>(entity =>
        {
            entity.Property(e => e.PrintConfigHeaderStandardCode).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<PrintConfigStandard>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.Property(e => e.PrintConfigName).HasMaxLength(255);
            entity.Property(e => e.TemplateFileName).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<PrintConfigStandardModuleType>(entity =>
        {
            entity.HasKey(e => new { e.PrintConfigStandardId, e.ModuleTypeId });

            entity.HasOne(d => d.PrintConfigStandard).WithMany(p => p.PrintConfigStandardModuleType)
                .HasForeignKey(d => d.PrintConfigStandardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintConfigStandardModuleType_PrintConfigStandard");
        });

        modelBuilder.Entity<ReuseContent>(entity =>
        {
            entity.Property(e => e.Key).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);
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

            entity.HasOne(d => d.ParentRole).WithMany(p => p.InverseParentRole)
                .HasForeignKey(d => d.ParentRoleId)
                .HasConstraintName("FK_Role_Role");
        });

        modelBuilder.Entity<RoleDataPermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ObjectTypeId, e.ObjectId });

            entity.HasOne(d => d.Role).WithMany(p => p.RoleDataPermission)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleDataPermission_Role");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.ModuleId, e.ObjectTypeId, e.ObjectId });

            entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Module).WithMany(p => p.RolePermission)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermission_Module");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermission)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermission_Role");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(e => new { e.UnitName, e.SubsidiaryId }, "IX_Unit_UnitName")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.DecimalPlace).HasDefaultValueSql("((12))");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.UnitStatusId).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => new { e.SubsidiaryId, e.UserName }, "IX_User_UserName")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0) AND [UserName]<>'' AND [UserName] IS NOT NULL)");

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

        modelBuilder.Entity<UserProgramingFunction>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.FunctionBody).IsRequired();
            entity.Property(e => e.ProgramingFunctionName)
                .IsRequired()
                .HasMaxLength(128);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
