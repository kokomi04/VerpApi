using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class RoleStatus
    {
        public RoleStatus()
        {
            Role = new HashSet<Role>();
        }

        public int RoleStatusId { get; set; }
        public string RoleStatusName { get; set; }

        public virtual ICollection<Role> Role { get; set; }
    }
}
