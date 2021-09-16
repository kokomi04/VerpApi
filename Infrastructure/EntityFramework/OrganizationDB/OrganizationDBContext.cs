using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBContext : DbContext
    {
        public OrganizationDBContext()
        {
        }

        public OrganizationDBContext(DbContextOptions<OrganizationDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerAttachment> CustomerAttachment { get; set; }
        public virtual DbSet<CustomerBankAccount> CustomerBankAccount { get; set; }
        public virtual DbSet<CustomerContact> CustomerContact { get; set; }
        public virtual DbSet<DayOffCalendar> DayOffCalendar { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<DepartmentCapacityBalance> DepartmentCapacityBalance { get; set; }
        public virtual DbSet<DepartmentDayOffCalendar> DepartmentDayOffCalendar { get; set; }
        public virtual DbSet<DepartmentOverHourInfo> DepartmentOverHourInfo { get; set; }
        public virtual DbSet<DepartmentWorkingHourInfo> DepartmentWorkingHourInfo { get; set; }
        public virtual DbSet<DepartmentWorkingWeekInfo> DepartmentWorkingWeekInfo { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual DbSet<EmployeeSubsidiary> EmployeeSubsidiary { get; set; }
        public virtual DbSet<HrAction> HrAction { get; set; }
        public virtual DbSet<HrArea> HrArea { get; set; }
        public virtual DbSet<HrAreaField> HrAreaField { get; set; }
        public virtual DbSet<HrField> HrField { get; set; }
        public virtual DbSet<HrType> HrType { get; set; }
        public virtual DbSet<HrTypeGlobalSetting> HrTypeGlobalSetting { get; set; }
        public virtual DbSet<HrTypeGroup> HrTypeGroup { get; set; }
        public virtual DbSet<ObjectProcessObject> ObjectProcessObject { get; set; }
        public virtual DbSet<ObjectProcessStep> ObjectProcessStep { get; set; }
        public virtual DbSet<ObjectProcessStepDepend> ObjectProcessStepDepend { get; set; }
        public virtual DbSet<ObjectProcessStepUser> ObjectProcessStepUser { get; set; }
        public virtual DbSet<Subsidiary> Subsidiary { get; set; }
        public virtual DbSet<SystemParameter> SystemParameter { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }
        public virtual DbSet<WorkingHourInfo> WorkingHourInfo { get; set; }
        public virtual DbSet<WorkingWeekInfo> WorkingWeekInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<BusinessInfo>(entity =>
            {
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LegalRepresentative)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.TaxIdNo)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Website).HasMaxLength(128);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.CustomerCode }, "IX_Customer_CustomerCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.Address).HasMaxLength(128);

                entity.Property(e => e.CustomerCode).HasMaxLength(128);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CustomerStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.DebtLimitation).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.Identify).HasMaxLength(64);

                entity.Property(e => e.LegalRepresentative).HasMaxLength(128);

                entity.Property(e => e.LoanLimitation).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.PartnerId).HasMaxLength(32);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.Property(e => e.Website).HasMaxLength(128);
            });

            modelBuilder.Entity<CustomerAttachment>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(256);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerAttachment)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerAttachment_Customer");
            });

            modelBuilder.Entity<CustomerBankAccount>(entity =>
            {
                entity.Property(e => e.AccountName).HasMaxLength(255);

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BankBranch)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BankCode)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BankName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Province).HasMaxLength(255);

                entity.Property(e => e.SwiffCode)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerBankAccount)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankAccount_Customer");
            });

            modelBuilder.Entity<CustomerContact>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.FullName).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.Position).HasMaxLength(128);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerContact)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerContact_Customer");
            });

            modelBuilder.Entity<DayOffCalendar>(entity =>
            {
                entity.HasKey(e => new { e.SubsidiaryId, e.Day });

                entity.Property(e => e.Content).HasMaxLength(255);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.DepartmentCode }, "IX_Department_DepartmentCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.DepartmentName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.WorkingHoursPerDay).HasColumnType("decimal(4, 2)");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Department_SelfKey");
            });

            modelBuilder.Entity<DepartmentCapacityBalance>(entity =>
            {
                entity.HasIndex(e => new { e.DepartmentId, e.StartDate, e.EndDate }, "UCI_DepartmentCapacityBalance")
                    .IsUnique();

                entity.Property(e => e.WorkingHours).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.DepartmentCapacityBalance)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DepartmentCapacityBalance_Department");
            });

            modelBuilder.Entity<DepartmentDayOffCalendar>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.SubsidiaryId, e.Day });

                entity.Property(e => e.Content).HasMaxLength(255);
            });

            modelBuilder.Entity<DepartmentOverHourInfo>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(255);
            });

            modelBuilder.Entity<DepartmentWorkingHourInfo>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.StartDate, e.SubsidiaryId });
            });

            modelBuilder.Entity<DepartmentWorkingWeekInfo>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.DayOfWeek, e.SubsidiaryId, e.StartDate });
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasIndex(e => new { e.SubsidiaryId, e.EmployeeCode }, "IX_Employee_EmployeeCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.Address).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.EmployeeCode).HasMaxLength(64);

                entity.Property(e => e.FullName).HasMaxLength(128);

                entity.Property(e => e.PartnerId).HasMaxLength(32);

                entity.Property(e => e.Phone).HasMaxLength(64);

                entity.Property(e => e.UserStatusId).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<EmployeeDepartmentMapping>(entity =>
            {
                entity.HasKey(e => e.UserDepartmentMappingId)
                    .HasName("PK__UserDepa__3E005141E85C5CB7");

                entity.Property(e => e.EffectiveDate).HasColumnType("date");

                entity.Property(e => e.ExpirationDate).HasColumnType("date");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.EmployeeDepartmentMapping)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeDepartment_Department");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmployeeDepartmentMapping)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeDepartment_Employee");
            });

            modelBuilder.Entity<EmployeeSubsidiary>(entity =>
            {
                entity.HasOne(d => d.Subsidiary)
                    .WithMany(p => p.EmployeeSubsidiary)
                    .HasForeignKey(d => d.SubsidiaryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeSubsidiary_Subsidiary");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmployeeSubsidiary)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeSubsidiary_Employee");
            });

            modelBuilder.Entity<HrAction>(entity =>
            {
                entity.Property(e => e.HractionCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("HRActionCode");

                entity.Property(e => e.IconName).HasMaxLength(25);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.HrType)
                    .WithMany(p => p.HrAction)
                    .HasForeignKey(d => d.HrTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HRAction_HRType");
            });

            modelBuilder.Entity<HrArea>(entity =>
            {
                entity.Property(e => e.Columns).HasDefaultValueSql("((1))");

                entity.Property(e => e.HrAreaCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.HrType)
                    .WithMany(p => p.HrArea)
                    .HasForeignKey(d => d.HrTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HRArea_HRType");
            });

            modelBuilder.Entity<HrAreaField>(entity =>
            {
                entity.HasIndex(e => new { e.HrTypeId, e.HrFieldId }, "IX_InputAreaField")
                    .IsUnique();

                entity.Property(e => e.Column).HasDefaultValueSql("((1))");

                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.InputStyleJson).HasMaxLength(512);

                entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.ReferenceUrl).HasMaxLength(1024);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.TitleStyleJson).HasMaxLength(512);

                entity.HasOne(d => d.HrArea)
                    .WithMany(p => p.HrAreaField)
                    .HasForeignKey(d => d.HrAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HRAreaField_HRArea");

                entity.HasOne(d => d.HrField)
                    .WithMany(p => p.HrAreaField)
                    .HasForeignKey(d => d.HrFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HRAreaField_HRField");

                entity.HasOne(d => d.HrType)
                    .WithMany(p => p.HrAreaField)
                    .HasForeignKey(d => d.HrTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HRAreaField_HRType");
            });

            modelBuilder.Entity<HrField>(entity =>
            {
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

                entity.HasOne(d => d.HrArea)
                    .WithMany(p => p.HrField)
                    .HasForeignKey(d => d.HrAreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HrField_HrArea");
            });

            modelBuilder.Entity<HrType>(entity =>
            {
                entity.Property(e => e.HrTypeCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.HrTypeGroup)
                    .WithMany(p => p.HrType)
                    .HasForeignKey(d => d.HrTypeGroupId)
                    .HasConstraintName("FK_HRType_HRTypeGroup");
            });

            modelBuilder.Entity<HrTypeGroup>(entity =>
            {
                entity.Property(e => e.HrTypeGroupName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ObjectProcessObject>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(512);
            });

            modelBuilder.Entity<ObjectProcessStep>(entity =>
            {
                entity.Property(e => e.ObjectProcessStepName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ObjectProcessStepDepend>(entity =>
            {
                entity.HasKey(e => new { e.ObjectProcessStepId, e.DependObjectProcessStepId });

                entity.HasOne(d => d.DependObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepDependDependObjectProcessStep)
                    .HasForeignKey(d => d.DependObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep1");

                entity.HasOne(d => d.ObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepDependObjectProcessStep)
                    .HasForeignKey(d => d.ObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep");
            });

            modelBuilder.Entity<ObjectProcessStepUser>(entity =>
            {
                entity.HasKey(e => new { e.ObjectProcessStepId, e.UserId });

                entity.HasOne(d => d.ObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepUser)
                    .HasForeignKey(d => d.ObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepUser_ObjectProcessStep");
            });

            modelBuilder.Entity<Subsidiary>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.Fax).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.SubsidiaryCode).HasMaxLength(128);

                entity.Property(e => e.SubsidiaryName).HasMaxLength(128);

                entity.Property(e => e.SubsidiaryStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.HasOne(d => d.ParentSubsidiary)
                    .WithMany(p => p.InverseParentSubsidiary)
                    .HasForeignKey(d => d.ParentSubsidiaryId)
                    .HasConstraintName("FK_Subsidiary_Subsidiary");
            });

            modelBuilder.Entity<SystemParameter>(entity =>
            {
                entity.Property(e => e.CreatedDateTimeUtc).HasColumnType("datetime");

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.Property(e => e.UpdatedDateTimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Value).HasMaxLength(512);
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.DataKey });

                entity.Property(e => e.DataKey).HasMaxLength(128);
            });

            modelBuilder.Entity<WorkingHourInfo>(entity =>
            {
                entity.HasKey(e => new { e.StartDate, e.SubsidiaryId })
                    .HasName("PK_WorkingHourHistory");
            });

            modelBuilder.Entity<WorkingWeekInfo>(entity =>
            {
                entity.HasKey(e => new { e.DayOfWeek, e.SubsidiaryId, e.StartDate })
                    .HasName("PK_WorkingWeek");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
