﻿using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOutputBasic
    {
        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
    }

    public class PurchaseOrderOutputList : PurchaseOrderOutputBasic
    {
        public long? Date { get; set; }
        public int CustomerId { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public string Requirement { get; set; }
        public string DeliveryPolicy { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
        public EnumPurchaseOrderStatus PurchaseOrderStatusId { get; set; }
        public bool? IsChecked { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public string PoDescription { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CheckedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public long? CheckedDatetimeUtc { get; set; }

        public int PurchaseOrderType { get; set; }
        public long? DeliveryDate { get; set; }

        public long? PropertyCalcId { get; set; }

        public long? CurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }

        public string DeliveryMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string AttachmentBill { get; set; }
        public EnumInputType? InputTypeSelectedState { get; set; }
        public EnumInputUnitType? InputUnitTypeSelectedState { get; set; }
    }

    public class PurchaseOrderOutput : PurchaseOrderOutputList
    {
        public string OtherPolicy { get; set; }

        //public long? DeliveryDate { get; set; }
        public int? DeliveryUserId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        public IList<long> FileIds { get; set; }
        public IList<PurchaseOrderOutputDetail> Details { get; set; }

        public IList<PurchaseOrderExcessModel> Excess { get; set; }
        public IList<PurchaseOrderMaterialsModel> Materials { get; set; }
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


        // public decimal? TaxInMoney { get; set; }
        public string PoProviderPricingCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }

        public PoAssignmentDetailInfo PoAssignmentDetail { get; set; }
        public PurchasingSuggestDetailInfo PurchasingSuggestDetail { get; set; }
        public string CreatorFullName { get; set; }
        public string CheckerFullName { get; set; }
        public string CensorFullName { get; set; }

        public decimal? IntoMoney { get; set; }
        // public decimal? IntoAfterTaxMoney { get; set; }

        // public string CurrencyCode { get; set; }
        // public decimal? ExchangeRate { get; set; }
        public decimal? ExchangedMoney { get; set; }
        public bool IsSubCalculation { get; set; }
    }

}
