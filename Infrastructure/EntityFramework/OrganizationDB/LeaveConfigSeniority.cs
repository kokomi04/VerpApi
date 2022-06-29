#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class LeaveConfigSeniority
    {
        public int LeaveConfigId { get; set; }
        public int Months { get; set; }
        public int AdditionDays { get; set; }
    }
}
