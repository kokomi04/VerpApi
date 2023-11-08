using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSheetAggregateAbsenceModel : IMapFrom<TimeSheetAggregateAbsence>
    {
        public long TimeSheetAggregateId { get; set; }

        public int AbsenceTypeSymbolId { get; set; }

        public int CountedDay { get; set; }

    }
}