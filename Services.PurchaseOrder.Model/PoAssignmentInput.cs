using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PoAssignmentInput
    {
        public int AssigneeUserId { get; set; }
        public string PoAssignmentCode { get; set; }
        public string Content { get; set; }
        public IList<PoAssimentDetailModel> Details { get; set; }
    }

    public class PoAssimentDetailModel
    {
        public long? PoAssignmentDetailId { get; set; }
        public long PurchasingSuggestDetailId { get; set; }
        public string ProviderProductName { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? Tax { get; set; }
    }
}
