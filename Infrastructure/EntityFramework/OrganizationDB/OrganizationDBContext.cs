using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OrganizationDBContext : DbContext
{
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

    public virtual DbSet<OvertimeConfigurationMapping> OvertimeConfigurationMapping { get; set; }

    public virtual DbSet<OvertimeLevel> OvertimeLevel { get; set; }


    public virtual DbSet<SalaryField> SalaryField { get; set; }

    public virtual DbSet<SalaryGroup> SalaryGroup { get; set; }

    public virtual DbSet<SalaryGroupField> SalaryGroupField { get; set; }

    public virtual DbSet<SalaryPeriod> SalaryPeriod { get; set; }

    public virtual DbSet<SalaryPeriodAdditionBill> SalaryPeriodAdditionBill { get; set; }

    public virtual DbSet<SalaryPeriodAdditionBillEmployee> SalaryPeriodAdditionBillEmployee { get; set; }

    public virtual DbSet<SalaryPeriodAdditionBillEmployeeValue> SalaryPeriodAdditionBillEmployeeValue { get; set; }

    public virtual DbSet<SalaryPeriodAdditionField> SalaryPeriodAdditionField { get; set; }

    public virtual DbSet<SalaryPeriodAdditionType> SalaryPeriodAdditionType { get; set; }

    public virtual DbSet<SalaryPeriodAdditionTypeField> SalaryPeriodAdditionTypeField { get; set; }

    public virtual DbSet<SalaryPeriodGroup> SalaryPeriodGroup { get; set; }

    public virtual DbSet<SalaryRefTable> SalaryRefTable { get; set; }

    public virtual DbSet<ShiftConfiguration> ShiftConfiguration { get; set; }

    public virtual DbSet<ShiftSchedule> ShiftSchedule { get; set; }

    public virtual DbSet<ShiftScheduleConfiguration> ShiftScheduleConfiguration { get; set; }

    public virtual DbSet<ShiftScheduleDetail> ShiftScheduleDetail { get; set; }

    public virtual DbSet<SplitHour> SplitHour { get; set; }

    public virtual DbSet<Subsidiary> Subsidiary { get; set; }

    public virtual DbSet<SystemParameter> SystemParameter { get; set; }

    public virtual DbSet<TimeSheet> TimeSheet { get; set; }

    public virtual DbSet<TimeSheetAggregate> TimeSheetAggregate { get; set; }

    public virtual DbSet<TimeSheetAggregateAbsence> TimeSheetAggregateAbsence { get; set; }

    public virtual DbSet<TimeSheetAggregateOvertime> TimeSheetAggregateOvertime { get; set; }

    public virtual DbSet<TimeSheetDepartment> TimeSheetDepartment { get; set; }

    public virtual DbSet<TimeSheetDetail> TimeSheetDetail { get; set; }

    public virtual DbSet<TimeSheetDetailShift> TimeSheetDetailShift { get; set; }

    public virtual DbSet<TimeSheetDetailShiftCounted> TimeSheetDetailShiftCounted { get; set; }

    public virtual DbSet<TimeSheetDetailShiftOvertime> TimeSheetDetailShiftOvertime { get; set; }

    public virtual DbSet<TimeSheetRaw> TimeSheetRaw { get; set; }

    public virtual DbSet<TimeSortConfiguration> TimeSortConfiguration { get; set; }

    public virtual DbSet<UserData> UserData { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedule { get; set; }

    public virtual DbSet<WorkScheduleMark> WorkScheduleMark { get; set; }

    public virtual DbSet<WorkingHourInfo> WorkingHourInfo { get; set; }

    public virtual DbSet<WorkingWeekInfo> WorkingWeekInfo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbsenceTypeSymbol>(entity =>
        {
            entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsCounted)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MaxOfDaysOffPerMonth).HasDefaultValueSql("((1))");
            entity.Property(e => e.SalaryRate).HasDefaultValueSql("((1))");
            entity.Property(e => e.SymbolCode)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.TypeSymbolDescription)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<ArrangeShift>(entity =>
        {
            entity.HasOne(d => d.WorkSchedule).WithMany(p => p.ArrangeShift)
                .HasForeignKey(d => d.WorkScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArrangeShift_WorkSchedule");
        });

        modelBuilder.Entity<ArrangeShiftItem>(entity =>
        {
            entity.HasOne(d => d.ArrangeShift).WithMany(p => p.ArrangeShiftItem)
                .HasForeignKey(d => d.ArrangeShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArrangeShiftItem_ArrangeShift");

            entity.HasOne(d => d.ParentArrangeShiftItem).WithMany(p => p.InverseParentArrangeShiftItem)
                .HasForeignKey(d => d.ParentArrangeShiftItemId)
                .HasConstraintName("FK_ArrangeShiftItem_ArrangeShiftItem");

            entity.HasOne(d => d.ShiftConfiguration).WithMany(p => p.ArrangeShiftItem)
                .HasForeignKey(d => d.ShiftConfigurationId)
                .HasConstraintName("FK_ArrangeShiftItem_ShiftConfiguration");
        });

        modelBuilder.Entity<BusinessInfo>(entity =>
        {
            entity.HasKey(e => e.BusinessInfoId).HasName("PK__Business__1B6A2BF9B9BD420A");

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
            entity.Property(e => e.CountedPriority).HasDefaultValueSql("((1))");
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

            entity.HasOne(d => d.CustomerCate).WithMany(p => p.Customer)
                .HasForeignKey(d => d.CustomerCateId)
                .HasConstraintName("FK_Customer_CustomerCate");
        });

        modelBuilder.Entity<CustomerAttachment>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(256);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAttachment)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerAttachment_Customer");
        });

        modelBuilder.Entity<CustomerBankAccount>(entity =>
        {
            entity.HasKey(e => e.CustomerBankAccountId).HasName("PK__Customer__329F62FCD370C088");

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

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerBankAccount)
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

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerContact)
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
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BEDE09F3D27");

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

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Department_SelfKey");
        });

        modelBuilder.Entity<DepartmentCalendar>(entity =>
        {
            entity.HasKey(e => new { e.StartDate, e.DepartmentId });
        });

        modelBuilder.Entity<DepartmentCapacityBalance>(entity =>
        {
            entity.HasIndex(e => new { e.DepartmentId, e.StartDate, e.EndDate }, "UCI_DepartmentCapacityBalance").IsUnique();

            entity.Property(e => e.WorkingHours).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.Department).WithMany(p => p.DepartmentCapacityBalance)
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

            entity.HasOne(d => d.LeaveConfig).WithMany(p => p.Employee)
                .HasForeignKey(d => d.LeaveConfigId)
                .HasConstraintName("FK_Employee_LeaveConfig");
        });

        modelBuilder.Entity<EmployeeDepartmentMapping>(entity =>
        {
            entity.HasKey(e => e.UserDepartmentMappingId).HasName("PK__UserDepa__3E005141E85C5CB7");

            entity.Property(e => e.EffectiveDate).HasColumnType("date");
            entity.Property(e => e.ExpirationDate).HasColumnType("date");

            entity.HasOne(d => d.Department).WithMany(p => p.EmployeeDepartmentMapping)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeDepartment_Department");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeDepartmentMapping)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeDepartment_Employee");
        });

        modelBuilder.Entity<EmployeeSubsidiary>(entity =>
        {
            entity.HasOne(d => d.Subsidiary).WithMany(p => p.EmployeeSubsidiary)
                .HasForeignKey(d => d.SubsidiaryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeSubsidiary_Subsidiary");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeSubsidiary)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeSubsidiary_Employee");
        });

        modelBuilder.Entity<HrArea>(entity =>
        {
            entity.HasKey(e => e.HrAreaId).HasName("PK_InputArea");

            entity.Property(e => e.Columns).HasDefaultValueSql("((1))");
            entity.Property(e => e.HrAreaCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.HrType).WithMany(p => p.HrArea)
                .HasForeignKey(d => d.HrTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HRArea_HRType");
        });

        modelBuilder.Entity<HrAreaField>(entity =>
        {
            entity.HasKey(e => e.HrAreaFieldId).HasName("PK__InputAre__C2457D60A15F171E");

            entity.HasIndex(e => new { e.HrTypeId, e.HrFieldId }, "IX_InputAreaField").IsUnique();

            entity.Property(e => e.Column).HasDefaultValueSql("((1))");
            entity.Property(e => e.CustomButtonHtml).HasMaxLength(128);
            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.FiltersName).HasMaxLength(255);
            entity.Property(e => e.InputStyleJson).HasMaxLength(512);
            entity.Property(e => e.OnBlur).HasDefaultValueSql("('')");
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.ReferenceUrl).HasMaxLength(1024);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.RequireFiltersName).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);
            entity.Property(e => e.TitleStyleJson).HasMaxLength(512);
        });

        modelBuilder.Entity<HrBill>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("PK_HrValueBill");

            entity.Property(e => e.FId).HasColumnName("F_Id");
            entity.Property(e => e.BillCode).HasMaxLength(512);

            entity.HasOne(d => d.HrType).WithMany(p => p.HrBill)
                .HasForeignKey(d => d.HrTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HrValueBill_HrType");
        });

        modelBuilder.Entity<HrField>(entity =>
        {
            entity.HasKey(e => e.HrFieldId).HasName("PK_InputField");

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
            entity.Property(e => e.SqlValue).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.HrArea).WithMany(p => p.HrField)
                .HasForeignKey(d => d.HrAreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HrField_HrArea");
        });

        modelBuilder.Entity<HrType>(entity =>
        {
            entity.HasKey(e => e.HrTypeId).HasName("PK_InputType");

            entity.Property(e => e.HrTypeCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.HrTypeGroup).WithMany(p => p.HrType)
                .HasForeignKey(d => d.HrTypeGroupId)
                .HasConstraintName("FK_HRType_HRTypeGroup");
        });

        modelBuilder.Entity<HrTypeGlobalSetting>(entity =>
        {
            entity.HasKey(e => e.HrTypeGlobalSettingId).HasName("PK_InputTypeGlobalSetting");
        });

        modelBuilder.Entity<HrTypeGroup>(entity =>
        {
            entity.HasKey(e => e.HrTypeGroupId).HasName("PK_InputTypeGroup");

            entity.Property(e => e.HrTypeGroupName)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<HrTypeView>(entity =>
        {
            entity.Property(e => e.HrTypeViewName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(d => d.HrType).WithMany(p => p.HrTypeView)
                .HasForeignKey(d => d.HrTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HrTypeView_HrType");
        });

        modelBuilder.Entity<HrTypeViewField>(entity =>
        {
            entity.HasKey(e => e.HrTypeViewFieldId).HasName("PK_HrTypeViewField_1");

            entity.Property(e => e.DefaultValue).HasMaxLength(512);
            entity.Property(e => e.Placeholder).HasMaxLength(128);
            entity.Property(e => e.RefTableCode).HasMaxLength(128);
            entity.Property(e => e.RefTableField).HasMaxLength(128);
            entity.Property(e => e.RefTableTitle).HasMaxLength(512);
            entity.Property(e => e.RegularExpression).HasMaxLength(256);
            entity.Property(e => e.SelectFilters).HasMaxLength(512);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.HrTypeView).WithMany(p => p.HrTypeViewField)
                .HasForeignKey(d => d.HrTypeViewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HrTypeViewField_HrTypeView");
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.ToTable(tb => tb.HasComment("Đơn xin nghỉ phép"));

            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(128);
            entity.Property(e => e.TotalDays).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.TotalDaysLastYearUsed).HasColumnType("decimal(4, 1)");

            entity.HasOne(d => d.AbsenceTypeSymbol).WithMany(p => p.Leave)
                .HasForeignKey(d => d.AbsenceTypeSymbolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leave_AbsenceTypeSymbol");

            entity.HasOne(d => d.LeaveConfig).WithMany(p => p.Leave)
                .HasForeignKey(d => d.LeaveConfigId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leave_LeaveConfig");
        });

        modelBuilder.Entity<LeaveConfig>(entity =>
        {
            entity.ToTable(tb => tb.HasComment(""));

            entity.Property(e => e.AdvanceDays).HasComment("Số ngày được ứng trước");
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.MaxAyear)
                .HasComment("Số ngày phép tối đa 1 năm")
                .HasColumnName("MaxAYear");
            entity.Property(e => e.MonthRate)
                .HasComment("1 tháng làm việc được cho mấy ngày phép")
                .HasColumnType("decimal(4, 1)");
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
            entity.HasOne(d => d.LeaveConfig).WithMany(p => p.LeaveConfigRole)
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

            entity.HasOne(d => d.LeaveConfig).WithMany(p => p.LeaveConfigValidation)
                .HasForeignKey(d => d.LeaveConfigId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveConfigValidation_LeaveConfig");
        });

        modelBuilder.Entity<ObjectApprovalStep>(entity =>
        {
            entity.HasIndex(e => new { e.ObjectTypeId, e.ObjectId, e.ObjectApprovalStepTypeId, e.SubsidiaryId }, "Idx_ApproveStep").IsUnique();

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
            entity.HasIndex(e => new { e.ObjectProcessStepId, e.DependObjectProcessStepId }, "IX_ObjectProcessStepDepend").IsUnique();

            entity.HasOne(d => d.DependObjectProcessStep).WithMany(p => p.ObjectProcessStepDependDependObjectProcessStep)
                .HasForeignKey(d => d.DependObjectProcessStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep1");

            entity.HasOne(d => d.ObjectProcessStep).WithMany(p => p.ObjectProcessStepDependObjectProcessStep)
                .HasForeignKey(d => d.ObjectProcessStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep");
        });

        modelBuilder.Entity<ObjectProcessStepUser>(entity =>
        {
            entity.HasKey(e => new { e.ObjectProcessStepId, e.UserId });

            entity.HasOne(d => d.ObjectProcessStep).WithMany(p => p.ObjectProcessStepUser)
                .HasForeignKey(d => d.ObjectProcessStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ObjectProcessStepUser_ObjectProcessStep");
        });

        modelBuilder.Entity<OvertimeConfigurationMapping>(entity =>
        {
            entity.HasKey(e => new { e.OvertimeConfigurationId, e.OvertimeLevelId }).HasName("PK__Overtime__F94FAF4E88EEADFA");

            entity.HasOne(d => d.OvertimeConfiguration).WithMany(p => p.OvertimeConfigurationMapping)
                .HasForeignKey(d => d.OvertimeConfigurationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OvertimeC__Overt__4DB54E83");

            entity.HasOne(d => d.OvertimeLevel).WithMany(p => p.OvertimeConfigurationMapping)
                .HasForeignKey(d => d.OvertimeLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OvertimeC__Overt__4EA972BC");
        });

        modelBuilder.Entity<OvertimeLevel>(entity =>
        {
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.OvertimeCode)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.OvertimePriority).HasDefaultValueSql("((2))");
            entity.Property(e => e.OvertimeRate).HasColumnType("decimal(5, 2)");
        });


        modelBuilder.Entity<SalaryField>(entity =>
        {
            entity.Property(e => e.DecimalPlace).HasDefaultValueSql("((2))");
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

            entity.Property(e => e.GroupName).HasMaxLength(128);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.SalaryField).WithMany(p => p.SalaryGroupField)
                .HasForeignKey(d => d.SalaryFieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryGroupField_SalaryField");

            entity.HasOne(d => d.SalaryGroup).WithMany(p => p.SalaryGroupField)
                .HasForeignKey(d => d.SalaryGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryGroupField_SalaryGroup");
        });

        modelBuilder.Entity<SalaryPeriod>(entity =>
        {
            entity.HasIndex(e => new { e.Year, e.Month, e.SubsidiaryId }, "IX_SalaryPeriod")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");
        });

        modelBuilder.Entity<SalaryPeriodAdditionBill>(entity =>
        {
            entity.HasIndex(e => new { e.BillCode, e.SubsidiaryId }, "IDX_SalaryPeriodAdditionType_BillCode")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.BillCode)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Content).HasMaxLength(512);

            entity.HasOne(d => d.SalaryPeriodAdditionType).WithMany(p => p.SalaryPeriodAdditionBill)
                .HasForeignKey(d => d.SalaryPeriodAdditionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionBill_SalaryPeriodAdditionType");
        });

        modelBuilder.Entity<SalaryPeriodAdditionBillEmployee>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);

            entity.HasOne(d => d.SalaryPeriodAdditionBill).WithMany(p => p.SalaryPeriodAdditionBillEmployee)
                .HasForeignKey(d => d.SalaryPeriodAdditionBillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionBillEmployee_SalaryPeriodAdditionBill");
        });

        modelBuilder.Entity<SalaryPeriodAdditionBillEmployeeValue>(entity =>
        {
            entity.HasKey(e => new { e.SalaryPeriodAdditionBillEmployeeId, e.SalaryPeriodAdditionFieldId });

            entity.Property(e => e.Value).HasColumnType("decimal(32, 12)");

            entity.HasOne(d => d.SalaryPeriodAdditionBillEmployee).WithMany(p => p.SalaryPeriodAdditionBillEmployeeValue)
                .HasForeignKey(d => d.SalaryPeriodAdditionBillEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionBillEmployeeValue_SalaryPeriodAdditionBillEmployee");

            entity.HasOne(d => d.SalaryPeriodAdditionField).WithMany(p => p.SalaryPeriodAdditionBillEmployeeValue)
                .HasForeignKey(d => d.SalaryPeriodAdditionFieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionBillEmployeeValue_SalaryPeriodAdditionField");
        });

        modelBuilder.Entity<SalaryPeriodAdditionField>(entity =>
        {
            entity.HasIndex(e => e.FieldName, "IX_SalaryPeriodAdditionField")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.DecimalPlace).HasDefaultValueSql("((2))");
            entity.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<SalaryPeriodAdditionType>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<SalaryPeriodAdditionTypeField>(entity =>
        {
            entity.HasKey(e => new { e.SalaryPeriodAdditionTypeId, e.SalaryPeriodAdditionFieldId });

            entity.HasOne(d => d.SalaryPeriodAdditionField).WithMany(p => p.SalaryPeriodAdditionTypeField)
                .HasForeignKey(d => d.SalaryPeriodAdditionFieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionTypeField_SalaryPeriodAdditionField");

            entity.HasOne(d => d.SalaryPeriodAdditionType).WithMany(p => p.SalaryPeriodAdditionTypeField)
                .HasForeignKey(d => d.SalaryPeriodAdditionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodAdditionTypeField_SalaryPeriodAdditionType");
        });

        modelBuilder.Entity<SalaryPeriodGroup>(entity =>
        {
            entity.HasIndex(e => new { e.SalaryPeriodId, e.SalaryGroupId }, "IX_SalaryPeriodGroup")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.HasOne(d => d.SalaryGroup).WithMany(p => p.SalaryPeriodGroup)
                .HasForeignKey(d => d.SalaryGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalaryPeriodGroup_SalaryGroup");

            entity.HasOne(d => d.SalaryPeriod).WithMany(p => p.SalaryPeriodGroup)
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
            entity.HasKey(e => e.ShiftConfigurationId).HasName("PK_Shift");

            entity.Property(e => e.ConfirmationUnit).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.IsCheckOutDateTimekeeping).HasDefaultValueSql("((0))");
            entity.Property(e => e.ShiftCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.OvertimeConfiguration).WithMany(p => p.ShiftConfiguration)
                .HasForeignKey(d => d.OvertimeConfigurationId)
                .HasConstraintName("FK_ShiftConfiguration_OvertimeConfiguration");
        });

        modelBuilder.Entity<ShiftSchedule>(entity =>
        {
            entity.HasKey(e => e.ShiftScheduleId).HasName("PK__ShiftSch__93A9B590EC9DA6AB");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<ShiftScheduleConfiguration>(entity =>
        {
            entity.HasKey(e => new { e.ShiftScheduleId, e.ShiftConfigurationId }).HasName("PK__ShiftSch__204A69BC5822DCC8");

            entity.HasOne(d => d.ShiftSchedule).WithMany(p => p.ShiftScheduleConfiguration)
                .HasForeignKey(d => d.ShiftScheduleId)
                .HasConstraintName("FK__ShiftSche__Shift__522FEADD");
        });

        modelBuilder.Entity<ShiftScheduleDetail>(entity =>
        {
            entity.HasKey(e => new { e.ShiftScheduleId, e.ShiftConfigurationId, e.AssignedDate, e.EmployeeId }).HasName("PK__ShiftSch__CA713DD74AB698E8");

            entity.HasOne(d => d.ShiftSchedule).WithMany(p => p.ShiftScheduleDetail)
                .HasForeignKey(d => d.ShiftScheduleId)
                .HasConstraintName("FK__ShiftSche__Shift__57E8C433");
        });

        modelBuilder.Entity<SplitHour>(entity =>
        {
            entity.HasOne(d => d.TimeSortConfiguration).WithMany(p => p.SplitHour)
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

            entity.HasOne(d => d.ParentSubsidiary).WithMany(p => p.InverseParentSubsidiary)
                .HasForeignKey(d => d.ParentSubsidiaryId)
                .HasConstraintName("FK_Subsidiary_Subsidiary");
        });

        modelBuilder.Entity<SystemParameter>(entity =>
        {
            entity.HasKey(e => e.SystemParameterId).HasName("PK_SystemParamter");

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
            entity.Property(e => e.Note).HasMaxLength(1024);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(512);
        });

        modelBuilder.Entity<TimeSheetAggregate>(entity =>
        {
            entity.Property(e => e.CountedHoliday).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.CountedHolidayHour).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.CountedWeekday).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.CountedWeekdayHour).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.CountedWeekend).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.CountedWeekendHour).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.TimeSheet).WithMany(p => p.TimeSheetAggregate)
                .HasForeignKey(d => d.TimeSheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeSheetAggregate_TimeSheet");
        });

        modelBuilder.Entity<TimeSheetAggregateAbsence>(entity =>
        {
            entity.HasKey(e => new { e.TimeSheetAggregateId, e.AbsenceTypeSymbolId }).HasName("PK__TimeShee__7E7CA50059A8709C");

            entity.HasOne(d => d.TimeSheetAggregate).WithMany(p => p.TimeSheetAggregateAbsence)
                .HasForeignKey(d => d.TimeSheetAggregateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheet__TimeS__0351120B");
        });

        modelBuilder.Entity<TimeSheetAggregateOvertime>(entity =>
        {
            entity.HasKey(e => new { e.TimeSheetAggregateId, e.OvertimeLevelId }).HasName("PK__TimeShee__B3138CBBADFD1E96");

            entity.Property(e => e.CountedMins).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.TimeSheetAggregate).WithMany(p => p.TimeSheetAggregateOvertime)
                .HasForeignKey(d => d.TimeSheetAggregateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheet__TimeS__0074A560");
        });

        modelBuilder.Entity<TimeSheetDepartment>(entity =>
        {
            entity.HasKey(e => new { e.TimeSheetId, e.DepartmentId }).HasName("PK__TimeShee__CD052EF4C6BCC0AC");

            entity.HasOne(d => d.TimeSheet).WithMany(p => p.TimeSheetDepartment)
                .HasForeignKey(d => d.TimeSheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheet__TimeS__72268609");
        });

        modelBuilder.Entity<TimeSheetDetail>(entity =>
        {
            entity.HasOne(d => d.TimeSheet).WithMany(p => p.TimeSheetDetail)
                .HasForeignKey(d => d.TimeSheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeSheetDetail_TimeSheet");
        });

        modelBuilder.Entity<TimeSheetDetailShift>(entity =>
        {
            entity.HasKey(e => new { e.TimeSheetDetailId, e.ShiftConfigurationId }).HasName("PK__TimeShee__3F25922631AE7536");

            entity.Property(e => e.WorkCounted).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.TimeSheetDetail).WithMany(p => p.TimeSheetDetailShift)
                .HasForeignKey(d => d.TimeSheetDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheet__TimeS__360845B8");
        });

        modelBuilder.Entity<TimeSheetDetailShiftCounted>(entity =>
        {
            entity.HasKey(e => e.TimeSheetDetailShiftCountedId).HasName("PK__TimeShee__E7FF6B3A52601E70");

            entity.HasOne(d => d.TimeSheetDetailShift).WithMany(p => p.TimeSheetDetailShiftCounted)
                .HasForeignKey(d => new { d.TimeSheetDetailId, d.ShiftConfigurationId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheetDetailS__38E4B263");
        });

        modelBuilder.Entity<TimeSheetDetailShiftOvertime>(entity =>
        {
            entity.HasKey(e => new { e.TimeSheetDetailId, e.ShiftConfigurationId, e.OvertimeLevelId, e.OvertimeType }).HasName("PK__TimeShee__19101DA10B09318B");

            entity.HasOne(d => d.TimeSheetDetailShift).WithMany(p => p.TimeSheetDetailShiftOvertime)
                .HasForeignKey(d => new { d.TimeSheetDetailId, d.ShiftConfigurationId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TimeSheetDetailS__3BC11F0E");
        });

        modelBuilder.Entity<TimeSheetRaw>(entity =>
        {
            entity.Property(e => e.TimeKeepingRecorder)
                .IsRequired()
                .HasMaxLength(128)
                .HasDefaultValueSql("('')");
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

            entity.HasOne(d => d.TimeSortConfiguration).WithMany(p => p.WorkSchedule)
                .HasForeignKey(d => d.TimeSortConfigurationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkSchedule_TimeSortConfiguration");
        });

        modelBuilder.Entity<WorkScheduleMark>(entity =>
        {
            entity.HasKey(e => e.WorkScheduleMarkId).HasName("PK_WorkScheduleHistory");

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkScheduleMark)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkScheduleHistory_WorkScheduleHistory");

            entity.HasOne(d => d.WorkSchedule).WithMany(p => p.WorkScheduleMark)
                .HasForeignKey(d => d.WorkScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkScheduleMark_WorkSchedule");
        });

        modelBuilder.Entity<WorkingHourInfo>(entity =>
        {
            entity.HasKey(e => new { e.StartDate, e.SubsidiaryId, e.CalendarId }).HasName("PK_WorkingHourHistory");
        });

        modelBuilder.Entity<WorkingWeekInfo>(entity =>
        {
            entity.HasKey(e => new { e.DayOfWeek, e.SubsidiaryId, e.StartDate, e.CalendarId }).HasName("PK_WorkingWeek");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
