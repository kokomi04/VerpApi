using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ActionButton
    {
        public ActionButton()
        {
            ActionButtonBillType = new HashSet<ActionButtonBillType>();
        }

        public int ActionButtonId { get; set; }
        public int? ObjectTypeIdBak { get; set; }
        public long? ObjectIdBak { get; set; }
        public int? BillTypeObjectTypeId { get; set; }
        public string ActionButtonCode { get; set; }
        public string Title { get; set; }
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
        public int ActionPositionId { get; set; }

        public virtual ICollection<ActionButtonBillType> ActionButtonBillType { get; set; }
    }
}
