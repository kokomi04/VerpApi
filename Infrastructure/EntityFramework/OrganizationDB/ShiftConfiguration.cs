using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftConfiguration
{
    public int ShiftConfigurationId { get; set; }

    public int? OvertimeConfigurationId { get; set; }

    public string ShiftCode { get; set; }

    public string Description { get; set; }

    public TimeSpan EntryTime { get; set; }

    public TimeSpan ExitTime { get; set; }

    public bool IsNightShift { get; set; }

    public bool? IsCheckOutDateTimekeeping { get; set; }

    public TimeSpan LunchTimeStart { get; set; }

    public TimeSpan LunchTimeFinish { get; set; }

    public long ConvertToMins { get; set; }

    public decimal ConfirmationUnit { get; set; }

    public TimeSpan StartTimeOnRecord { get; set; }

    public TimeSpan EndTimeOnRecord { get; set; }

    public TimeSpan StartTimeOutRecord { get; set; }

    public TimeSpan EndTimeOutRecord { get; set; }

    public int WorkScheduleId { get; set; }

    public bool IsSkipSaturdayWithShift { get; set; }

    public bool IsSkipSundayWithShift { get; set; }

    public bool IsSkipHolidayWithShift { get; set; }

    public bool IsCountWorkForHoliday { get; set; }

    public int PartialShiftCalculationMode { get; set; }

    public bool IsSubtractionForLate { get; set; }

    public bool IsSubtractionForEarly { get; set; }

    public long MinsAllowToLate { get; set; }

    public long MinsAllowToEarly { get; set; }

    public bool IsCalculationForLate { get; set; }

    public bool IsCalculationForEarly { get; set; }

    public long MinsRoundForLate { get; set; }

    public long MinsRoundForEarly { get; set; }

    public bool IsRoundBackForLate { get; set; }

    public bool IsRoundBackForEarly { get; set; }

    public int? MaxLateMins { get; set; }

    public int? MaxEarlyMins { get; set; }

    public bool IsExceededLateAbsenceType { get; set; }

    public bool IsExceededEarlyAbsenceType { get; set; }

    public int? ExceededLateAbsenceTypeId { get; set; }

    public int? ExceededEarlyAbsenceTypeId { get; set; }

    public int? NoEntryTimeAbsenceTypeId { get; set; }

    public int? NoExitTimeAbsenceTypeId { get; set; }

    public bool IsNoEntryTimeWorkMins { get; set; }

    public bool IsNoExitTimeWorkMins { get; set; }

    public long? NoEntryTimeWorkMins { get; set; }

    public long? NoExitTimeWorkMins { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<ArrangeShiftItem> ArrangeShiftItem { get; set; } = new List<ArrangeShiftItem>();

    public virtual OvertimeConfiguration OvertimeConfiguration { get; set; }
}
