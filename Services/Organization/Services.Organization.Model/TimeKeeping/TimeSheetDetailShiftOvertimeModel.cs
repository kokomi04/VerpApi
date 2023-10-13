using AutoMapper;
using System;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetDetailShiftOvertimeModel : IMapFrom<TimeSheetDetailShiftOvertime>
    {
        public long TimeSheetDetailId { get; set; }

        public int ShiftConfigurationId { get; set; }

        public int OvertimeLevelId { get; set; }

        public long MinsOvertime { get; set; }

        public EnumTimeSheetOvertimeType OvertimeType { get; set; }
    }
}
