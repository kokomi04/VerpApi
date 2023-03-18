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

        public virtual DbSet<AbsenceTypeSymbol> AbsenceTypeSymbol { get; set; }
        public virtual DbSet<ArrangeShift> ArrangeShift { get; set; }
        public virtual DbSet<ArrangeShiftItem> ArrangeShiftItem { get; set; }
        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<Calendar> Calendar { get; set; }
        public virtual DbSet<CountedSymbol> CountedSymbol { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerAttachment> CustomerAttachment { get; set; }
        public virtual DbSet<CustomerBankAccount> CustomerBankAccount { get; set; }
        public virtual DbSet<CustomerCate> CustomerCate { get; set; }
        public virtual DbSet<CustomerContact> CustomerContact { get; set; }
        public virtual DbSet<DayOffCalendar> DayOffCalendar { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<DepartmentCalendar> DepartmentCalendar { get; set; }
        public virtual DbSet<DepartmentCapacityBalance> DepartmentCapacityBalance { get; set; }
        public virtual DbSet<DepartmentIncreaseInfo> DepartmentIncreaseInfo { get; set; }
        public virtual DbSet<DepartmentOverHourInfo> DepartmentOverHourInfo { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual DbSet<EmployeeSubsidiary> EmployeeSubsidiary { get; set; }
        public virtual DbSet<HrArea> HrArea { get; set; }
        public virtual DbSet<HrAreaField> HrAreaField { get; set; }
        public virtual DbSet<HrBill> HrBill { get; set; }
        public virtual DbSet<HrField> HrField { get; set; }
        public virtual DbSet<HrType> HrType { get; set; }
        public virtual DbSet<HrTypeGlobalSetting> HrTypeGlobalSetting { get; set; }
        public virtual DbSet<HrTypeGroup> HrTypeGroup { get; set; }
        public virtual DbSet<HrTypeView> HrTypeView { get; set; }
        public virtual DbSet<HrTypeViewField> HrTypeViewField { get; set; }
        public virtual DbSet<Leave> Leave { get; set; }
        public virtual DbSet<LeaveConfig> LeaveConfig { get; set; }
        public virtual DbSet<LeaveConfigRole> LeaveConfigRole { get; set; }
        public virtual DbSet<LeaveConfigSeniority> LeaveConfigSeniority { get; set; }
        public virtual DbSet<LeaveConfigValidation> LeaveConfigValidation { get; set; }
        public virtual DbSet<ObjectApprovalStep> ObjectApprovalStep { get; set; }
        public virtual DbSet<ObjectProcessObject> ObjectProcessObject { get; set; }
        public virtual DbSet<ObjectProcessStep> ObjectProcessStep { get; set; }
        public virtual DbSet<ObjectProcessStepDepend> ObjectProcessStepDepend { get; set; }
        public virtual DbSet<ObjectProcessStepUser> ObjectProcessStepUser { get; set; }
        public virtual DbSet<OvertimeConfiguration> OvertimeConfiguration { get; set; }
        public virtual DbSet<OvertimeLevel> OvertimeLevel { get; set; }
        public virtual DbSet<SalaryEmployee> SalaryEmployee { get; set; }
        public virtual DbSet<SalaryEmployeeValue> SalaryEmployeeValue { get; set; }
        public virtual DbSet<SalaryField> SalaryField { get; set; }
        public virtual DbSet<SalaryGroup> SalaryGroup { get; set; }
        public virtual DbSet<SalaryGroupField> SalaryGroupField { get; set; }
        public virtual DbSet<SalaryPeriod> SalaryPeriod { get; set; }
        public virtual DbSet<SalaryPeriodGroup> SalaryPeriodGroup { get; set; }
        public virtual DbSet<SalaryRefTable> SalaryRefTable { get; set; }
        public virtual DbSet<ShiftConfiguration> ShiftConfiguration { get; set; }
        public virtual DbSet<SplitHour> SplitHour { get; set; }
        public virtual DbSet<Subsidiary> Subsidiary { get; set; }
        public virtual DbSet<SystemParameter> SystemParameter { get; set; }
        public virtual DbSet<TimeSheet> TimeSheet { get; set; }
        public virtual DbSet<TimeSheetAggregate> TimeSheetAggregate { get; set; }
        public virtual DbSet<TimeSheetDayOff> TimeSheetDayOff { get; set; }
        public virtual DbSet<TimeSheetDetail> TimeSheetDetail { get; set; }
        public virtual DbSet<TimeSheetOvertime> TimeSheetOvertime { get; set; }
        public virtual DbSet<TimeSheetRaw> TimeSheetRaw { get; set; }
        public virtual DbSet<TimeSortConfiguration> TimeSortConfiguration { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }
        public virtual DbSet<WorkSchedule> WorkSchedule { get; set; }
        public virtual DbSet<WorkScheduleMark> WorkScheduleMark { get; set; }
        public virtual DbSet<WorkingHourInfo> WorkingHourInfo { get; set; }
        public virtual DbSet<WorkingWeekInfo> WorkingWeekInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AbsenceTypeSymbol>(entity =>
            {
                entity.Property(e => e.SymbolCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TypeSymbolCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TypeSymbolDescription)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ArrangeShift>(entity =>
            {
                entity.HasOne(d => d.WorkSchedule)
                    .WithMany(p => p.ArrangeShift)
                    .HasForeignKey(d => d.WorkScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ArrangeShift_WorkSchedule");
            });

            modelBuilder.Entity<ArrangeShiftItem>(entity =>
            {
                entity.HasOne(d => d.ArrangeShift)
                    .WithMany(p => p.ArrangeShiftItem)
                    .HasForeignKey(d => d.ArrangeShiftId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ArrangeShiftItem_ArrangeShift");

                entity.HasOne(d => d.ParentArrangeShiftItem)
                    .WithMany(p => p.InverseParentArrangeShiftItem)
                    .HasForeignKey(d => d.ParentArrangeShiftItemId)
                    .HasConstraintName("FK_ArrangeShiftItem_ArrangeShiftItem");

                entity.HasOne(d => d.ShiftConfiguration)
                    .WithMany(p => p.ArrangeShiftItem)
                    .HasForeignKey(d => d.ShiftConfigurationId)
                    .HasConstraintName("FK_ArrangeShiftItem_ShiftConfiguration");
            });

            modelBuilder.Entity<BusinessInfo>(entity =>
            {
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.AddressEng).HasMaxLength(128);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CompanyNameEng).HasMaxLength(128);

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

            modelBuilder.Entity<Calendar>(entity =>
            {
                entity.Property(e => e.CalendarCode)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.CalendarName).HasMaxLength(64);
            });

            modelBuilder.Entity<CountedSymbol>(entity =>
            {
                entity.Property(e => e.SymbolCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SymbolDescription)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.CustomerCode }, "IX_Customer_CustomerCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.HasIndex(e => new { e.IsDeleted, e.CustomerStatusId, e.PartnerId }, "Idx_Customer_PartnerId");

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

                entity.HasOne(d => d.CustomerCate)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.CustomerCateId)
                    .HasConstraintName("FK_Customer_CustomerCate");
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

            modelBuilder.Entity<CustomerCate>(entity =>
            {
                entity.Property(e => e.CustomerCateCode).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(1024);

                entity.Property(e => e.Name).HasMaxLength(256);
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
                entity.HasKey(e => new { e.SubsidiaryId, e.Day, e.CalendarId });

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

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Department_SelfKey");
            });

            modelBuilder.Entity<DepartmentCalendar>(entity =>
            {
                entity.HasKey(e => new { e.StartDate, e.DepartmentId });
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

            modelBuilder.Entity<DepartmentIncreaseInfo>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(255);
            });

            modelBuilder.Entity<DepartmentOverHourInfo>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(255);
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

                entity.HasOne(d => d.LeaveConfig)
                    .WithMany(p => p.Employee)
                    .HasForeignKey(d => d.LeaveConfigId)
                    .HasConstraintName("FK_Employee_LeaveConfig");
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

                entity.Property(e => e.CustomButtonHtml).HasMaxLength(128);

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

            modelBuilder.Entity<HrBill>(entity =>
            {
                entity.HasKey(e => e.FId)
                    .HasName("PK_HrValueBill");

                entity.Property(e => e.FId).HasColumnName("F_Id");

                entity.Property(e => e.BillCode).HasMaxLength(512);

                entity.HasOne(d => d.HrType)
                    .WithMany(p => p.HrBill)
                    .HasForeignKey(d => d.HrTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HrValueBill_HrType");
            });

            modelBuilder.Entity<HrField>(entity =>
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

            modelBuilder.Entity<HrTypeView>(entity =>
            {
                entity.Property(e => e.HrTypeViewName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.HrType)
                    .WithMany(p => p.HrTypeView)
                    .HasForeignKey(d => d.HrTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HrTypeView_HrType");
            });

            modelBuilder.Entity<HrTypeViewField>(entity =>
            {
                entity.Property(e => e.DefaultValue).HasMaxLength(512);

                entity.Property(e => e.Placeholder).HasMaxLength(128);

                entity.Property(e => e.RefTableCode).HasMaxLength(128);

                entity.Property(e => e.RefTableField).HasMaxLength(128);

                entity.Property(e => e.RefTableTitle).HasMaxLength(512);

                entity.Property(e => e.RegularExpression).HasMaxLength(256);

                entity.Property(e => e.SelectFilters).HasMaxLength(512);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.HasOne(d => d.HrTypeView)
                    .WithMany(p => p.HrTypeViewField)
                    .HasForeignKey(d => d.HrTypeViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HrTypeViewField_HrTypeView");
            });

            modelBuilder.Entity<Leave>(entity =>
            {
                entity.HasComment("Đơn xin nghỉ phép");

                entity.Property(e => e.Description).HasMaxLength(1024);

                entity.Property(e => e.Title).HasMaxLength(128);

                entity.Property(e => e.TotalDays).HasColumnType("decimal(4, 1)");

                entity.Property(e => e.TotalDaysLastYearUsed).HasColumnType("decimal(4, 1)");

                entity.HasOne(d => d.AbsenceTypeSymbol)
                    .WithMany(p => p.Leave)
                    .HasForeignKey(d => d.AbsenceTypeSymbolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Leave_AbsenceTypeSymbol");

                entity.HasOne(d => d.LeaveConfig)
                    .WithMany(p => p.Leave)
                    .HasForeignKey(d => d.LeaveConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Leave_LeaveConfig");
            });

            modelBuilder.Entity<LeaveConfig>(entity =>
            {
                entity.HasComment("");

                entity.Property(e => e.AdvanceDays).HasComment("Số ngày được ứng trước");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.MaxAyear)
                    .HasColumnName("MaxAYear")
                    .HasComment("Số ngày phép tối đa 1 năm");

                entity.Property(e => e.MonthRate)
                    .HasColumnType("decimal(4, 1)")
                    .HasComment("1 tháng làm việc được cho mấy ngày phép");

                entity.Property(e => e.OldYearAppliedToDate).HasComment("Phép năm cũ sẽ áp dụng đến ngày tháng nào");

                entity.Property(e => e.OldYearTransferMax).HasComment("Số phép tối đa mà năm cũ chuyển sang");

                entity.Property(e => e.SeniorityMonthsStart).HasComment("Làm đến tháng thứ mấy thì bắt đầu tính thâm niên");

                entity.Property(e => e.SeniorityOneYearRate).HasComment("Bắt đầu tính thâm niên từ tháng mấy của năm");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<LeaveConfigRole>(entity =>
            {
                entity.HasOne(d => d.LeaveConfig)
                    .WithMany(p => p.LeaveConfigRole)
                    .HasForeignKey(d => d.LeaveConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LeaveConfigRole_LeaveConfig");
            });

            modelBuilder.Entity<LeaveConfigSeniority>(entity =>
            {
                entity.HasKey(e => new { e.LeaveConfigId, e.Months });
            });

            modelBuilder.Entity<LeaveConfigValidation>(entity =>
            {
                entity.HasKey(e => new { e.LeaveConfigId, e.TotalDays });

                entity.HasOne(d => d.LeaveConfig)
                    .WithMany(p => p.LeaveConfigValidation)
                    .HasForeignKey(d => d.LeaveConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LeaveConfigValidation_LeaveConfig");
            });

            modelBuilder.Entity<ObjectApprovalStep>(entity =>
            {
                entity.HasIndex(e => new { e.ObjectTypeId, e.ObjectId, e.ObjectApprovalStepTypeId, e.SubsidiaryId }, "Idx_ApproveStep")
                    .IsUnique();

                entity.Property(e => e.ObjectFieldEnable).HasMaxLength(1024);
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

            modelBuilder.Entity<OvertimeLevel>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(1024);

                entity.Property(e => e.OvertimeRate).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Title).HasMaxLength(1024);
            });

            modelBuilder.Entity<SalaryEmployee>(entity =>
            {
                entity.HasIndex(e => new { e.SalaryPeriodId, e.EmployeeId }, "IX_SalaryEmployee")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.SalaryEmployee)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryEmployee_HrBill");

                entity.HasOne(d => d.SalaryGroup)
                    .WithMany(p => p.SalaryEmployee)
                    .HasForeignKey(d => d.SalaryGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryEmployee_SalaryGroup");

                entity.HasOne(d => d.SalaryPeriod)
                    .WithMany(p => p.SalaryEmployee)
                    .HasForeignKey(d => d.SalaryPeriodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryEmployee_SalaryPeriod");
            });

            modelBuilder.Entity<SalaryEmployeeValue>(entity =>
            {
                entity.HasKey(e => new { e.SalaryEmployeeId, e.SalaryFieldId });

                entity.Property(e => e.Value).HasColumnType("sql_variant");

                entity.HasOne(d => d.SalaryEmployee)
                    .WithMany(p => p.SalaryEmployeeValue)
                    .HasForeignKey(d => d.SalaryEmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryEmployeeValue_SalaryEmployee");

                entity.HasOne(d => d.SalaryField)
                    .WithMany(p => p.SalaryEmployeeValue)
                    .HasForeignKey(d => d.SalaryFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryEmployeeValue_SalaryField");
            });

            modelBuilder.Entity<SalaryField>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.GroupName).HasMaxLength(128);

                entity.Property(e => e.SalaryFieldName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<SalaryGroup>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(128);
            });

            modelBuilder.Entity<SalaryGroupField>(entity =>
            {
                entity.HasKey(e => new { e.SalaryGroupId, e.SalaryFieldId });

                entity.HasOne(d => d.SalaryField)
                    .WithMany(p => p.SalaryGroupField)
                    .HasForeignKey(d => d.SalaryFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryGroupField_SalaryField");

                entity.HasOne(d => d.SalaryGroup)
                    .WithMany(p => p.SalaryGroupField)
                    .HasForeignKey(d => d.SalaryGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryGroupField_SalaryGroup");
            });

            modelBuilder.Entity<SalaryPeriod>(entity =>
            {
                entity.HasIndex(e => new { e.Year, e.Month, e.SubsidiaryId }, "IX_SalaryPeriod")
                    .IsUnique();
            });

            modelBuilder.Entity<SalaryPeriodGroup>(entity =>
            {
                entity.HasIndex(e => new { e.SalaryPeriodId, e.SalaryGroupId }, "IX_SalaryPeriodGroup")
                    .IsUnique();

                entity.HasOne(d => d.SalaryGroup)
                    .WithMany(p => p.SalaryPeriodGroup)
                    .HasForeignKey(d => d.SalaryGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryPeriodGroup_SalaryGroup");

                entity.HasOne(d => d.SalaryPeriod)
                    .WithMany(p => p.SalaryPeriodGroup)
                    .HasForeignKey(d => d.SalaryPeriodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalaryPeriodGroup_SalaryPeriod");
            });

            modelBuilder.Entity<SalaryRefTable>(entity =>
            {
                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Filter).HasMaxLength(1024);

                entity.Property(e => e.FromField)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.RefTableCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.RefTableField)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ShiftConfiguration>(entity =>
            {
                entity.Property(e => e.ConfirmationUnit).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ShiftCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.OvertimeConfiguration)
                    .WithMany(p => p.ShiftConfiguration)
                    .HasForeignKey(d => d.OvertimeConfigurationId)
                    .HasConstraintName("FK_ShiftConfiguration_OvertimeConfiguration");
            });

            modelBuilder.Entity<SplitHour>(entity =>
            {
                entity.HasOne(d => d.TimeSortConfiguration)
                    .WithMany(p => p.SplitHour)
                    .HasForeignKey(d => d.TimeSortConfigurationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SplitHour_SplitHour");
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

            modelBuilder.Entity<TimeSheet>(entity =>
            {
                entity.Property(e => e.Note)
                    .IsRequired()
                    .HasMaxLength(1024);
            });

            modelBuilder.Entity<TimeSheetAggregate>(entity =>
            {
                entity.Property(e => e.CountedWeekday).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.CountedWeekdayHour).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.CountedWeekend).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.CountedWeekendHour).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Overtime1).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Overtime2).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.Overtime3).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.TimeSheet)
                    .WithMany(p => p.TimeSheetAggregate)
                    .HasForeignKey(d => d.TimeSheetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSheetAggregate_TimeSheet");
            });

            modelBuilder.Entity<TimeSheetDayOff>(entity =>
            {
                entity.HasOne(d => d.TimeSheet)
                    .WithMany(p => p.TimeSheetDayOff)
                    .HasForeignKey(d => d.TimeSheetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSheetDayOff_TimeSheetDayOff");
            });

            modelBuilder.Entity<TimeSheetDetail>(entity =>
            {
                entity.HasOne(d => d.TimeSheet)
                    .WithMany(p => p.TimeSheetDetail)
                    .HasForeignKey(d => d.TimeSheetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSheetDetail_TimeSheet");
            });

            modelBuilder.Entity<TimeSheetOvertime>(entity =>
            {
                entity.Property(e => e.MinsOvertime).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.OvertimeLevel)
                    .WithMany(p => p.TimeSheetOvertime)
                    .HasForeignKey(d => d.OvertimeLevelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSheetOvertime_OvertimeLevel");

                entity.HasOne(d => d.TimeSheet)
                    .WithMany(p => p.TimeSheetOvertime)
                    .HasForeignKey(d => d.TimeSheetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeSheetOvertime_TimeSheet");
            });

            modelBuilder.Entity<TimeSortConfiguration>(entity =>
            {
                entity.Property(e => e.TimeSortCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TimeSortDescription)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.DataKey });

                entity.Property(e => e.DataKey).HasMaxLength(128);
            });

            modelBuilder.Entity<WorkSchedule>(entity =>
            {
                entity.Property(e => e.IsAbsenceForHoliday).HasColumnName("isAbsenceForHoliday");

                entity.Property(e => e.IsAbsenceForSunday).HasColumnName("isAbsenceForSunday");

                entity.Property(e => e.WorkScheduleTitle)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.HasOne(d => d.TimeSortConfiguration)
                    .WithMany(p => p.WorkSchedule)
                    .HasForeignKey(d => d.TimeSortConfigurationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WorkSchedule_TimeSortConfiguration");
            });

            modelBuilder.Entity<WorkScheduleMark>(entity =>
            {
                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.WorkScheduleMark)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WorkScheduleHistory_WorkScheduleHistory");

                entity.HasOne(d => d.WorkSchedule)
                    .WithMany(p => p.WorkScheduleMark)
                    .HasForeignKey(d => d.WorkScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WorkScheduleMark_WorkSchedule");
            });

            modelBuilder.Entity<WorkingHourInfo>(entity =>
            {
                entity.HasKey(e => new { e.StartDate, e.SubsidiaryId, e.CalendarId })
                    .HasName("PK_WorkingHourHistory");
            });

            modelBuilder.Entity<WorkingWeekInfo>(entity =>
            {
                entity.HasKey(e => new { e.DayOfWeek, e.SubsidiaryId, e.StartDate, e.CalendarId })
                    .HasName("PK_WorkingWeek");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
