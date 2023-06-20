using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class LeaveConfig
{
    public int LeaveConfigId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Số ngày được ứng trước
    /// </summary>
    public int AdvanceDays { get; set; }

    /// <summary>
    /// 1 tháng làm việc được cho mấy ngày phép
    /// </summary>
    public decimal? MonthRate { get; set; }

    /// <summary>
    /// Số ngày phép tối đa 1 năm
    /// </summary>
    public int? MaxAyear { get; set; }

    /// <summary>
    /// Làm đến tháng thứ mấy thì bắt đầu tính thâm niên
    /// </summary>
    public int? SeniorityMonthsStart { get; set; }

    /// <summary>
    /// Bắt đầu tính thâm niên từ tháng mấy của năm
    /// </summary>
    public int? SeniorityOneYearRate { get; set; }

    /// <summary>
    /// Số phép tối đa mà năm cũ chuyển sang
    /// </summary>
    public int? OldYearTransferMax { get; set; }

    /// <summary>
    /// Phép năm cũ sẽ áp dụng đến ngày tháng nào
    /// </summary>
    public DateTime? OldYearAppliedToDate { get; set; }

    public bool IsDefault { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<Employee> Employee { get; set; } = new List<Employee>();

    public virtual ICollection<Leave> Leave { get; set; } = new List<Leave>();

    public virtual ICollection<LeaveConfigRole> LeaveConfigRole { get; set; } = new List<LeaveConfigRole>();

    public virtual ICollection<LeaveConfigValidation> LeaveConfigValidation { get; set; } = new List<LeaveConfigValidation>();
}
