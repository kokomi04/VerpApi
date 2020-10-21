using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class OpeningBalanceModel
    {
        [Display(Name ="Danh mục mặt hàng")]
        public string CateName { set; get; }

        [Display(Name = "Loại mã mặt hàng")]
        public string CatePrefixCode { set; get; }

        [Display(Name = "Mã mặt hàng")]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng")]
        public string ProductName { set; get; }

        [Display(Name = "Đơn vị tính")]
        public string Unit1 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)")]
        public decimal Qty1 { set; get; }

        [Display(Name = "Giá (Đơn vị chính)")]
        public decimal UnitPrice { set; get; }

        [Display(Name = "Quy cách")]
        public string Specification { set; get; }

        [Display(Name = "Đơn vị chuyển đổi")]
        public string Unit2 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)")]
        public decimal Qty2 { set; get; }

        [Display(Name = "Tỷ lệ Đơn vị chuyển đổi")]
        public decimal Factor { set; get; }

        [Display(Name = "Chiều cao/dày mặt hàng")]
        public decimal Height { set; get; }

        [Display(Name = "Chiều rộng mặt hàng")]
        public decimal Width { set; get; }

        [Display(Name = "Chiều dài mặt hàng")]
        public decimal Long { set; get; }

        [Display(Name = "Tài khoản kế toán")]
        public string AccountancyAccountNumber { set; get; }

        [Display(Name = "Tài khoản kế toán đối ứng")]
        public string AccountancyAccountNumberDu { set; get; }

        

    }
}
