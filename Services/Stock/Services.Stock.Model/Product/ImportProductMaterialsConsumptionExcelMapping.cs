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
        [Required(ErrorMessage = "Vui lòng nhập mã mặt hàng")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string ProductName { get; set; }

        [Display(Name = "Đơn vị mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string UnitName { get; set; }

        [Display(Name = "Định danh loại mã mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string ProductTypeCode { get; set; }

        [Display(Name = "Quy cách mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string Specification { get; set; }


        [Display(Name = "Danh mục mặt hàng (Nếu có)", GroupName = "Thông tin chung")]
        public string ProductCateName { get; set; }


        [Display(Name = "Số lượng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public decimal Quantity { get; set; }


        [Display(Name = "Mã bộ phận (Nếu có)", GroupName = "Thông tin chung")]
        public string DepartmentCode { get; set; }
        [Display(Name = "Tên bộ phận (Nếu có)", GroupName = "Thông tin chung")]
        public string DepartmentName { get; set; }

        [Display(Name = "Tên công đoạn (Nếu có)", GroupName = "Thông tin chung")]
        public string StepName { get; set; }

        //[Display(Name = "Mã nhóm vật tư tiêu hao", GroupName = "Thông tin chung")]
        //[Required(ErrorMessage = "Vui lòng nhập mã nhóm vật tư tiêu hao")]
        //public string GroupCode { get; set; }

        //[Display(Name = "Tên nhóm vật tư tiêu hao(Nếu có)", GroupName = "Thông tin chung")]
        //public string GroupTitle { get; set; }
    }
}
