using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetAggregateOvertimeModel : IMapFrom<TimeSheetAggregateOvertime>
    {
        public long TimeSheetAggregateId { get; set; }

        public int OvertimeLevelId { get; set; }

        public decimal CountedMins { get; set; }
    }
}