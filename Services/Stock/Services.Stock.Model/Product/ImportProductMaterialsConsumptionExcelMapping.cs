using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    [Display(Name = "Vật tư tiêu hao")]
    public class ImportProductMaterialsConsumptionExcelMapping
    {

        [Display(Name = "Mã Nvl tiêu hao", GroupName = "Nvl tiêu hao")]
        [Required(ErrorMessage = "Vui lòng nhập mã Nvl tiêu hao")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên Nvl tiêu hao (Nếu có)", GroupName = "Nvl tiêu hao")]
        public string ProductName { get; set; }

        [Display(Name = "Đơn vị Nvl tiêu hao (Nếu có)", GroupName = "Nvl tiêu hao")]
        public string UnitName { get; set; }

        [Display(Name = "Định danh loại mã mặt hàng Nvl tiêu hao (Nếu có)", GroupName = "Nvl tiêu hao")]
        public string ProductTypeCode { get; set; }

        [Display(Name = "Quy cách Nvl tiêu hao (Nếu có)", GroupName = "Nvl tiêu hao")]
        public string Specification { get; set; }

        [Display(Name = "Danh mục Nvl tiêu hao (Nếu có)", GroupName = "Nvl tiêu hao")]
        public string ProductCateName { get; set; }


        [Display(Name = "Mã chi tiết sử dụng", GroupName = "Chi tiết sử dụng")]
        [Required(ErrorMessage = "Vui lòng nhập mã chi tiết sử dụng")]
        public string UsageProductCode { get; set; }

        [Display(Name = "Tên chi tiết sử dụng (Nếu có)", GroupName = "Chi tiết sử dụng")]
        public string UsageProductName { get; set; }

        [Display(Name = "Đơn vị chi tiết sử dụng (Nếu có)", GroupName = "Chi tiết sử dụng")]
        public string UsageUnitName { get; set; }

        [Display(Name = "Định danh loại mã mặt hàng của chi tiết sử dụng (Nếu có)", GroupName = "Chi tiết sử dụng")]
        public string UsageProductTypeCode { get; set; }

        [Display(Name = "Quy cách chi tiết sử dụng (Nếu có)", GroupName = "Chi tiết sử dụng")]
        public string UsageSpecification { get; set; }

        [Display(Name = "Danh mục chi tiết sử dụng (Nếu có)", GroupName = "Chi tiết sử dụng")]
        public string UsageProductCateName { get; set; }


        [Display(Name = "Tên nhóm vật tư tiêu hao", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm vật tư tiêu hao")]
        public string GroupTitle { get; set; }

        [Display(Name = "Số lượng sử dụng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng sử dụng")]
        public decimal Quantity { get; set; }


        [Display(Name = "Mã bộ phận (Nếu có)", GroupName = "Thông tin bộ phận")]
        public string DepartmentCode { get; set; }
        [Display(Name = "Tên bộ phận (Nếu có)", GroupName = "Thông tin bộ phận")]
        public string DepartmentName { get; set; }

        [Display(Name = "Tên công đoạn (Nếu có)", GroupName = "Thông tin công đoạn")]
        public string StepName { get; set; }

    }
}
