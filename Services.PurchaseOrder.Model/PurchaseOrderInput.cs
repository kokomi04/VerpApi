using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderInput
    {
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "")]
        [MinLength(1, ErrorMessage = "Vui lòng chọn mặt hàng")]
        public IList<PurchaseOrderInputDetail> Details { get; set; }
        public long Date { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã PO")]
        public string PurchaseOrderCode { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public string Content { get; set; }
        public string AdditionNote { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
    }

    public interface IPurchaseOrderInputDetail
    {
        long? PurchaseOrderDetailId { get; set; }
        long? PoAssignmentDetailId { get; set; }
        long? PurchasingSuggestDetailId { get; set; }

        string ProviderProductName { get; set; }

        int ProductId { get; set; }
        decimal PrimaryQuantity { get; set; }
        decimal? PrimaryUnitPrice { get; set; }
        decimal? TaxInPercent { get; set; }
        decimal? TaxInMoney { get; set; }      
    }

    public class PurchaseOrderInputDetail : IPurchaseOrderInputDetail
    {
        public long? PurchaseOrderDetailId { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public long? PoAssignmentDetailId { get; set; }
        public string ProviderProductName { get; set; }

        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        
    }

    public class DeliveryDestinationModel
    {
        public string DeliverTo { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string AdditionNote { get; set; }
    }
}
