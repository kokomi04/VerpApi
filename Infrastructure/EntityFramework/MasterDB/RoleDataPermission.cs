using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class RoleDataPermission
    {
        public int RoleId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }

        public virtual Role Role { get; set; }
    }
}
