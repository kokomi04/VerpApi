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

        [Display(Name = "Ghi chú", GroupName = "Khác")]
        public string Description { get; set; }

    }

    public class MaterialsConsumptionByProduct
    {
        public SimpleProduct RootProduct { get; set; }
        public IList<ProductMaterialsConsumptionPreview> MaterialsComsump { get; set; }
    }

    public class SimpleProduct
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string Specification { get; set; }
    }

    public class ProductMaterialsConsumptionPreview
    {
        public string GroupTitle { get; set; }
        public decimal Quantity { get; set; }
        public string StepName { get; set; }
        public string DepartmentName { get; set; }
        public decimal TotalQuantityInheritance { get; set; } = 0;
        public decimal BomQuantity { get; set; } = 1;

        public SimpleProduct ProductExtraInfo { get; set; }
        public SimpleProduct ProductMaterialsComsumptionExtraInfo { get; set; }

        public IList<ProductMaterialsConsumptionPreview> MaterialsConsumptionInherit { get; set; }

    }

    public class ProductMaterialsConsumptionPreviewComparer : IEqualityComparer<ProductMaterialsConsumptionPreview>
    {
        public bool Equals(ProductMaterialsConsumptionPreview x, ProductMaterialsConsumptionPreview y)
        {

            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.GroupTitle == y.GroupTitle && x.ProductMaterialsComsumptionExtraInfo.ProductCode == y.ProductMaterialsComsumptionExtraInfo.ProductCode;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(ProductMaterialsConsumptionPreview product)
        {
            if (Object.ReferenceEquals(product, null)) return 0;

            return product.GroupTitle.GetHashCode() ^ product.ProductMaterialsComsumptionExtraInfo.ProductCode.GetHashCode();
        }
    }
}
