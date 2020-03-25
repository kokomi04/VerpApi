using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class UserDepartmentMapping
    {
        public int UserDepartmentMappingId { get; set; }
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int UpdatedUserId { get; set; }

        public virtual Department Department { get; set; }
        public virtual User User { get; set; }
    }
}
