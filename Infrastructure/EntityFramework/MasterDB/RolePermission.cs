using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class RolePermission
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int Permission { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }

        public virtual Module Module { get; set; }
        public virtual Role Role { get; set; }
    }
}
