using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PoAssignmentOutputList
    {
        public long PoAssignmentId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public string OrderCode { get; set; }

        public string PoAssignmentCode { get; set; }
        public int AssigneeUserId { get; set; }
        public EnumPoAssignmentStatus PoAssignmentStatusId { get; set; }
        public bool? IsConfirmed { get; set; }
        public long CreatedDatetimeUtc { get; set; }
    }

    public class PoAssignmentOutput: PoAssignmentOutputList
    {
        public string Content { get; set; }
        public IList<PoAssimentDetailModel> Details { get; set; }
    }
}
