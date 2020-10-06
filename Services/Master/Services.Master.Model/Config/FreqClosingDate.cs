using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Master.Model.Config
{
    public class FreqClosingDate
    {
        public EnumFrequencyType Frequency { get; set; }
        [Range(1, 12, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int MonthOfYear { get; set; }
        [Range(1, 31, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int DayOfMonth { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        [Range(0, 23, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int HourInDay { get; set; }
        [Range(0, 59, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int MinuteInHour { get; set; }
    }
}
