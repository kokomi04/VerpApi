using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleStatusId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
