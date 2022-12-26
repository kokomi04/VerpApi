using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Library.Model;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{

    public class PurchaseOrderImportModel : PoDetailRowValue
    {
        [KeyCodeField]
        [Required]
        [Display(Name = "Mã đơn mua", GroupName = "TT Chung", Order = 1001)]        
        public string PurchaseOrderCode { get; set; }

        [ValidateDuplicateByKeyCode]
        [Required]
        [Display(Name = "Ngày đặt hàng", GroupName = "TT Chung", Order = 1002)]
        public DateTime? Date { get; set; }

        [ValidateDuplicateByKeyCode]
        [Required]
        [Display(Name = "Thời gian giao", GroupName = "TT Chung", Order = 1003)]
        public DateTime? DeliveryDate { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Nhà cung cấp", GroupName = "TT Chung", Order = 1004)]
        public ProviderCustomerImportModel CustomerInfo { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Nội dung", GroupName = "TT Chung", Order = 1005)]
        public string PoDescription { get; set; }


        [ValidateDuplicateByKeyCode]
        [Display(Name = "Thông tin liên hệ giao hàng", GroupName = "TT giao hàng", Order = 1006)]
        [FieldDataNestedObject]
        public DeliveryDestinationImportModel DeliveryInfo { get; set; }


        [ValidateDuplicateByKeyCode]
        [Display(Name = "Loại tiền", GroupName = "Tiền tệ", Order = 1007)]
        [DynamicCategoryMapping(CategoryCode = CurrencyCateConstants.CurrencyCategoryCode)]
        public CurrencyData Currency { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Tỷ giá", GroupName = "Tiền tệ", Order = 1008)]
        public decimal? ExchangeRate { get; set; }


        [ValidateDuplicateByKeyCode]
        [Display(Name = "Phí vận chuyển", GroupName = "Phí & Thuế", Order = 3001)]
        public decimal DeliveryFee { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Chi phí khác", GroupName = "Phí & Thuế", Order = 3002)]
        public decimal OtherFee { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Thuế (%)", GroupName = "Phí & Thuế", Order = 3002)]
        public decimal? TaxInPercent { get; set; }


        [ValidateDuplicateByKeyCode]
        [Display(Name = "Thuế thành tiền", GroupName = "Phí & Thuế", Order = 3003)]
        public decimal? TaxInMoney { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Tổng tiền", GroupName = "Tổng tiền", Order = 4001)]
        public decimal TotalMoney { get; set; }


        [ValidateDuplicateByKeyCode]
        [Display(Name = "Yêu cầu", GroupName = "TT Bổ sung", Order = 5001)]
        [MaxLength(512)]
        public string Requirement { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Chứng từ đi kèm", GroupName = "TT Bổ sung", Order = 5002)]
        [MaxLength(512)]
        public string AttachmentBill { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Điều khoản khác", GroupName = "TT Bổ sung", Order = 5003)]
        [MaxLength(512)]
        public string OtherPolicy { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Phương thức vận chuyển", GroupName = "TT Bổ sung", Order = 5004)]
        [MaxLength(512)]
        public string DeliveryMethod { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Điều kiện giao hàng", GroupName = "TT Bổ sung", Order = 5005)]
        [MaxLength(512)]
        public string DeliveryPolicy { get; set; }

        [ValidateDuplicateByKeyCode]
        [Display(Name = "Phương thức thanh toán", GroupName = "TT Bổ sung", Order = 5006)]
        [MaxLength(512)]
        public string PaymentMethod { get; set; }


        [FieldDataIgnore]
        public long? PropertyCalcId { get; set; } = null;

        [FieldDataIgnore]

        public EnumPurchasingOrderType PurchaseOrderType { get; set; } = EnumPurchasingOrderType.Default;

    }

    public class DeliveryDestinationImportModel : DeliveryDestinationModel
    {
        [Display(Name = "Nhân viên nhận", GroupName = "TT giao hàng", Order = 1)]
        public DeliveryUserImportModel DeliveryUser { get; set; }

        [Display(Name = "Đơn vị nhận", GroupName = "TT giao hàng", Order = 2)]
        [MaxLength(512)]
        public DeliveryCustomerImportModel DeliveryCustomer { get; set; }
    }

    public class DeliveryUserImportModel
    {
        [FieldDataIgnore]
        public int? DeliveryUserId { get; set; }
        [Display(Name = "Mã nhân viên", GroupName = "TT nhân viên", Order = 1)]
        public string EmployeeCode { get; set; }
        [Display(Name = "Họ tên nhân viên", GroupName = "TT nhân viên", Order = 1)]
        public string FullName { get; set; }
    }

    public class DeliveryCustomerImportModel
    {
        [FieldDataIgnore]
        public int? CustomerId { get; set; }

        [Display(Name = "Mã công ty", GroupName = "TT công ty", Order = 1)]
        public string CustomerCode { get; set; }

        [Display(Name = "Tên công ty", GroupName = "TT công ty", Order = 1)]
        public string CustomerName { get; set; }
    }

    public class ProviderCustomerImportModel
    {
        [FieldDataIgnore]
        public int? CustomerId { get; set; }

        [Display(Name = "Mã nhà cung cấp", GroupName = "TT nhà cung cấp", Order = 1)]
        public string CustomerCode { get; set; }

        [Display(Name = "Tên nhà cung cấp", GroupName = "TT nhà cung cấp", Order = 1)]
        public string CustomerName { get; set; }
    }
}
