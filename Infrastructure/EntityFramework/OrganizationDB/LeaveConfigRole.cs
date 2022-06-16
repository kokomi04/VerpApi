#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class LeaveConfigRole
    {
        public int LeaveConfigRoleId { get; set; }
        public int LeaveConfigId { get; set; }
        public int UserId { get; set; }
        public int LeaveRoleTypeId { get; set; }

        public virtual LeaveConfig LeaveConfig { get; set; }
    }
}
