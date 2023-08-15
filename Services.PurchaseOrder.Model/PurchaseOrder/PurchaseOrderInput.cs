using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderInput
    {
        public int CustomerId { get; set; }
        public IList<long> FileIds { get; set; }

        [Required(ErrorMessage = "")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn mặt hàng")]
        public IList<PurchaseOrderInputDetail> Details { get; set; }

        public IList<PurchaseOrderExcessModel> Excess { get; set; }
        public IList<PurchaseOrderMaterialsModel> Materials { get; set; }

        [Required]
        public long Date { get; set; }


        public string PurchaseOrderCode { get; set; }

        [MaxLength(512)]
        public string OtherPolicy { get; set; }

        public long? DeliveryDate { get; set; }
        public int? DeliveryUserId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        public DeliveryDestinationModel DeliveryDestination { get; set; }
        [MaxLength(512)]
        public string Requirement { get; set; }
        [MaxLength(512)]
        public string DeliveryPolicy { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
        public string PoDescription { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        public long? PropertyCalcId { get; set; }

        public long? CurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }

        public EnumPurchasingOrderType PurchaseOrderType { get; set; }

        public EnumPurchaseOrderInputType? InputTypeSelectedState { get; set; }
        public EnumPurchaseOrderInputUnitType? InputUnitTypeSelectedState { get; set; }

        [MaxLength(512)]
        public string DeliveryMethod { get; set; }
        [MaxLength(512)]
        public string PaymentMethod { get; set; }
        [MaxLength(512)]
        public string AttachmentBill { get; set; }


        public long UpdatedDatetimeUtc { get; set; }
    }

    public interface IPurchaseOrderInputDetail
    {
        long? PurchaseOrderDetailId { get; set; }
        long? PoAssignmentDetailId { get; set; }
        long? PurchasingSuggestDetailId { get; set; }

        string ProviderProductName { get; set; }

        int ProductId { get; set; }
        decimal PrimaryQuantity { get; set; }
        decimal PrimaryUnitPrice { get; set; }
        int ProductUnitConversionId { get; set; }
        decimal ProductUnitConversionQuantity { get; set; }
        decimal ProductUnitConversionPrice { get; set; }

        // decimal? TaxInPercent { get; set; }
        // decimal? TaxInMoney { get; set; }

        string PoProviderPricingCode { get; set; }
        string OrderCode { get; set; }
        string ProductionOrderCode { get; set; }

        string Description { get; set; }

        decimal? IntoMoney { get; set; }
        // decimal? IntoAfterTaxMoney { get; set; }

        // string CurrencyCode { get; set; }
        // decimal? ExchangeRate { get; set; }
        decimal? ExchangedMoney { get; set; }
        int? SortOrder { get; set; }

        bool IsSubCalculation { get; set; }
    }

    public class PurchaseOrderInputDetail : IPurchaseOrderInputDetail
    {
        public PurchaseOrderInputDetail()
        {
            OutsourceMappings = new List<PurchaseOrderOutsourceMappingModel>();
        }
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

        // public decimal? TaxInPercent { get; set; }
        // public decimal? TaxInMoney { get; set; }

        public string PoProviderPricingCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }

        public string Description { get; set; }

        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }

        public decimal? IntoMoney { get; set; }
        // public decimal? IntoAfterTaxMoney { get; set; }

        // public string CurrencyCode { get; set; }
        // public decimal? ExchangeRate { get; set; }
        public decimal? ExchangedMoney { get; set; }
        public int? SortOrder { get; set; }

        public bool IsSubCalculation { get; set; }

        public IList<PurchaseOrderDetailSubCalculationModel> SubCalculations { get; set; }
        public IList<PurchaseOrderOutsourceMappingModel> OutsourceMappings { get; set; }

    }

    public class DeliveryDestinationModel
    {
        [Display(Name = "Người nhận", GroupName = "TT giao hàng", Order = 3)]
        [MaxLength(512)]
        public string DeliverTo { get; set; }
        
        [Display(Name = "Tên công ty", GroupName = "TT giao hàng", Order = 4)]
        [MaxLength(512)]
        public string Company { get; set; }

        [Display(Name = "Địa chỉ", GroupName = "TT giao hàng", Order = 5)]
        [MaxLength(512)]
        public string Address { get; set; }

        [Display(Name = "Điện thoại", GroupName = "TT giao hàng", Order = 6)]
        [MaxLength(512)]
        public string Telephone { get; set; }

        [Display(Name = "Fax", GroupName = "TT giao hàng", Order = 7)]
        [MaxLength(512)]
        public string Fax { get; set; }

        [Display(Name = "Thông tin thêm", GroupName = "TT giao hàng", Order = 8)]
        [MaxLength(512)]
        public string AdditionNote { get; set; }
    }

   
}
