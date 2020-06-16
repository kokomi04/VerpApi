using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOutputBasic
    {
        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
    }

    public class PurchaseOrderOutputList: PurchaseOrderOutputBasic
    {
        public long? Date { get; set; }
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

    public class PurchaseOrderOutput : PurchaseOrderOutputList
    {
        public string PaymentInfo { get; set; }

        public long? DeliveryDate { get; set; }
        public int? DeliveryUserId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        public IList<PurchaseOrderOutputDetail> Details { get; set; }
    }

    public class PurchaseOrderOutputDetail : PurchaseOrderInputDetail
    {
        public PoAssignmentDetailInfo PoAssignmentDetail { get; set; }
        public PurchasingSuggestDetailInfo PurchasingSuggestDetail { get; set; }
    }

    public class PurchaseOrderOutputListByProduct : PurchaseOrderOutputList, IPurchaseOrderInputDetail
    {
        public long? PurchaseOrderDetailId { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public long? PoAssignmentDetailId { get; set; }
        public string ProviderProductName { get; set; }


        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }

        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }


        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        public PoAssignmentDetailInfo PoAssignmentDetail { get; set; }
        public PurchasingSuggestDetailInfo PurchasingSuggestDetail { get; set; }

    }

}
