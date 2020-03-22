using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOutputList
    {
        public long PurchaseOrder { get; set; }
        public string PurchaseOrderCode { get; set; }
        public long Date { get; set; }
        public int CustomerId { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public string Content { get; set; }
        public string AdditionNote { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
        public EnumPurchaseOrderStatus PurchaseOrderStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long? CensorDatetimeUtc { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }

    public class PurchaseOrderOutput: PurchaseOrderOutputList
    {
        public IList<PurchaseOrderOutputDetail> Details { get; set; }
    }

    public class PurchaseOrderOutputDetail
    {
        public long PurchaseOrderOutputDetailId { get; set; }
        public long PoAssignmentDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal TaxInPercent { get; set; }
        public decimal TaxInMoney { get; set; }
    }

}
