using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class OpeningBalanceModel
    {
        [Display(Name ="Danh mục mặt hàng", GroupName ="Sản phẩm")]
        public string CateName { set; get; }

        [Display(Name = "Loại mã mặt hàng", GroupName = "Sản phẩm")]
        public string CatePrefixCode { set; get; }

        [Display(Name = "Mã mặt hàng", GroupName = "Sản phẩm")]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng", GroupName = "Sản phẩm")]
        public string ProductName { set; get; }

        [Display(Name = "Chiều cao/dày mặt hàng", GroupName = "Sản phẩm")]
        public decimal Height { set; get; }

        [Display(Name = "Chiều rộng mặt hàng", GroupName = "Sản phẩm")]
        public decimal Width { set; get; }

        [Display(Name = "Chiều dài mặt hàng", GroupName = "Sản phẩm")]
        public decimal Long { set; get; }

        [Display(Name = "Quy cách", GroupName = "Sản phẩm")]
        public string Specification { set; get; }

        [Display(Name = "Đơn vị tính", GroupName = "Sản phẩm")]
        public string Unit1 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal Qty1 { set; get; }

        [Display(Name = "Giá (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal UnitPrice { set; get; }


        [Display(Name = "Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
        public string Unit2 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal Qty2 { set; get; }

        [Display(Name = "Tỷ lệ Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
        public decimal Factor { set; get; }


        [Display(Name = "Tài khoản kế toán", GroupName = "Thẻ Kho")]
        public string AccountancyAccountNumber { set; get; }

        [Display(Name = "Tài khoản kế toán đối ứng", GroupName = "Thẻ Kho")]
        public string AccountancyAccountNumberDu { set; get; }

        [Display(Name = "Mã kiện (Bỏ chọn nếu là mặc định)", GroupName = "Thẻ Kho")]

        [FieldDataType((int)EnumInventoryType.Output)]
        public string PackageCode { set; get; }

    }
}
