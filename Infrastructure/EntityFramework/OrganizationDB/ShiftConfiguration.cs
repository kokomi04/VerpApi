using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ShiftConfiguration
    {
        public int ShiftConfigurationId { get; set; }
        public int? OvertimeConfigurationId { get; set; }
        public string ShiftCode { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfTransition { get; set; }
        public TimeSpan LunchTimeStart { get; set; }
        public TimeSpan LunchTimeFinish { get; set; }
        public long ConvertToMins { get; set; }
        public decimal ConfirmationUnit { get; set; }
        public TimeSpan StartTimeOnRecord { get; set; }
        public TimeSpan EndTimeOnRecord { get; set; }
        public TimeSpan StartTimeOutRecord { get; set; }
        public TimeSpan EndTimeOutRecord { get; set; }
        public long MinsWithoutTimeOn { get; set; }
        public long MinsWithoutTimeOut { get; set; }
        public int PositionOnReport { get; set; }
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
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual OvertimeConfiguration OvertimeConfiguration { get; set; }
    }
}
