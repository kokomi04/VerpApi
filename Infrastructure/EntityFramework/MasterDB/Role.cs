using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Role
    {
        public Role()
        {
            RolePermission = new HashSet<RolePermission>();
        }

        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public int RoleStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEditable { get; set; }

        public virtual RoleStatus RoleStatus { get; set; }
        public virtual ICollection<RolePermission> RolePermission { get; set; }
    }
}
