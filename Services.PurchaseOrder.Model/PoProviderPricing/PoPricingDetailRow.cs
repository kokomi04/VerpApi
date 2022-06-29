using System.ComponentModel.DataAnnotations;
using VErp.Commons.Library.Model;

namespace VErp.Services.PurchaseOrder.Model.PoProviderPricing
{
    [Display(Name = "Chi tiết báo giá nhà cung cấp")]
    public class PoPricingDetailRow
    {
        [Display(Name = "Mã mặt hàng", GroupName = "Mặt hàng")]
        public string ProductCode { get; set; }
        [Display(Name = "Tên mặt hàng", GroupName = "Mặt hàng")]
        public string ProductName { get; set; }

        [FieldDataIgnore]
        public string ProductInternalName { get; set; }

        [Display(Name = "Tên mặt hàng tương ứng NCC", GroupName = "Mặt hàng")]
        public string ProductProviderName { get; set; }

        [Display(Name = "Số lượng Đơn vị chính", GroupName = "TT về lượng")]
        public decimal? PrimaryQuantity { get; set; }

        [Display(Name = "Giá theo đơn vị chính", GroupName = "TT về lượng")]
        public decimal? PrimaryPrice { get; set; }

        [Display(Name = "Tên Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public string ProductUnitConversionName { get; set; }

        [Display(Name = "Số lượng Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public decimal? ProductUnitConversionQuantity { get; set; }

        [Display(Name = "Giá theo Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public decimal? ProductUnitConversionPrice { get; set; }

        [Display(Name = "Thành tiền ngoại tệ", GroupName = "TT về lượng")]
        public decimal? IntoMoney { get; set; }

        [Display(Name = "Thành tiền quy đổi", GroupName = "TT về lượng")]
        public decimal? ExchangedMoney { get; set; }

        [Display(Name = "Mã báo giá nhà cung cấp", GroupName = "Bổ sung")]
        public string PoProviderPricingCode { get; set; }

        [Display(Name = "Mã đơn hàng", GroupName = "Bổ sung")]
        public string OrderCode { get; set; }

        [Display(Name = "Mã LSX", GroupName = "Bổ sung")]
        public string ProductionOrderCode { get; set; }

        [Display(Name = "Mô tả", GroupName = "Bổ sung")]
        public string Description { get; set; }

        [Display(Name = "Thứ tự sắp xếp", GroupName = "Bổ sung")]
        public int? SortOrder { get; set; }

    }
}
