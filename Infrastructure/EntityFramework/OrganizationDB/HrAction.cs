using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class HrAction
    {
        public int HrActionId { get; set; }
        public int HrTypeId { get; set; }
        public string Title { get; set; }
        public string HractionCode { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string SqlAction { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        public string JsVisible { get; set; }
        public int? ActionTypeId { get; set; }

        public virtual HrType HrType { get; set; }
    }
}
