using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimeLevelModel : IMapFrom<OvertimeLevel>
    {
        public int OvertimeLevelId { get; set; }
        public int OrdinalNumber { get; set; }
        public decimal OvertimeRate { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
    }
}