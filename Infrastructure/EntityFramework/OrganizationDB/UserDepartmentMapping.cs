using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class EmployeeDepartmentMapping
    {
        public int EmployeeDepartmentMappingId { get; set; }
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int UpdatedUserId { get; set; }

        public virtual Department Department { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
