using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    [Display(Name = "Vật tư tiêu hao")]
    public class ImportProductMaterialsConsumptionExcelMapping
    {
        [Display(Name = "Mã mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập mã mặt hàng chính")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string ProductName { get; set; }

        [Display(Name = "Đơn vị mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string UnitName { get; set; }

        [Display(Name = "Định danh loại mã mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string ProductTypeCode { get; set; }

        [Display(Name = "Quy cách mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string Specification { get; set; }

        [Display(Name = "Số lượng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public decimal Quantity { get; set; }
    }
}
