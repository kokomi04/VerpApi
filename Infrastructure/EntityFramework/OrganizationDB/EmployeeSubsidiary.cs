using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class EmployeeSubsidiary
    {
        public long EmployeeSubsidiaryId { get; set; }
        public int UserId { get; set; }
        public int SubsidiaryId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Subsidiary Subsidiary { get; set; }
        public virtual Employee User { get; set; }
    }
}
