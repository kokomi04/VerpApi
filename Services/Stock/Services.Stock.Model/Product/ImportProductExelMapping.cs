using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product
{
    [Display(Name = "Sản phẩm")]
    public class ProductImportModel
    {
        // General info
        [Display(Name = "Mã mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập mã mặt hàng")]
        [MaxLength(128, ErrorMessage = "Mã mặt hàng quá dài")]
        public string ProductCode { get; set; }
        [Display(Name = "Tên mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập tên mặt hàng")]
        [MaxLength(128, ErrorMessage = "Tên mặt hàng quá dài")]
        public string ProductName { get; set; }
        [Display(Name = "Mua", GroupName = "Thông tin chung")]
        public bool? IsCanBuy { get; set; }
        [Display(Name = "Bán", GroupName = "Thông tin chung")]
        public bool? IsCanSell { get; set; }
        [Display(Name = "Mã loại mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn mã loại mặt hàng")]
        public string ProductTypeCode { get; set; }
        [Display(Name = "Tên loại mặt hàng", GroupName = "Thông tin chung")]
        public string ProductTypeName { get; set; }
        [Display(Name = "Danh mục mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục mặt hàng")]
        public string ProductCate { get; set; }
        [Display(Name = "Cấu hình Barcode", GroupName = "Thông tin chung")]
        public int BarcodeConfigId { get; set; }
        [Display(Name = "Barcode", GroupName = "Thông tin chung")]
        public string Barcode { get; set; }
        [Display(Name = "Đơn vị chính", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn đơn vị chính")]
        public string Unit { get; set; }
        [Display(Name = "Giá ước tính", GroupName = "Thông tin chung")]
        public decimal? EstimatePrice { get; set; }
        [Display(Name = "Bán thành phẩm", GroupName = "Thông tin chung")]
        public bool? IsProductSemi { get; set; }
        [Display(Name = "Mặt hàng", GroupName = "Thông tin chung")]
        public bool? IsProduct { get; set; }
        [Display(Name = "Cơ số sản phẩm", GroupName = "Thông tin chung")]
        public int Coefficient { get; set; }

        // Extra info
        [Display(Name = "Quy cách", GroupName = "Thông tin bổ sung")]
        public string Specification { get; set; }
        [Display(Name = "Định lượng", GroupName = "Thông tin bổ sung")]
        public decimal Quantitative { get; set; }
        [Display(Name = "Đơn vị Định lượng(g/m2, g/m3)", GroupName = "Thông tin bổ sung")]
        public EnumQuantitativeUnitType? QuantitativeUnitTypeId { get; set; }

        [Display(Name = "Dài (mm)", GroupName = "Thông tin bổ sung")]
        public decimal Long { get; set; }

        [Display(Name = "Rộng (mm)", GroupName = "Thông tin bổ sung")]
        public decimal Width { get; set; }

        [Display(Name = "Cao (mm)", GroupName = "Thông tin bổ sung")]
        public decimal Height { get; set; }

        [Display(Name = "Thể tích (m3)", GroupName = "Thông tin bổ sung")]
        public decimal Measurement { get; set; }

        [Display(Name = "Trọng lượng tịnh (g)", GroupName = "Thông tin bổ sung")]
        public decimal NetWeight { get; set; }

        [Display(Name = "Tổng trọng lượng (g)", GroupName = "Thông tin bổ sung")]
        public decimal GrossWeight { get; set; }

        [Display(Name = "Tải trọng (g)", GroupName = "Thông tin bổ sung")]
        public decimal LoadAbility { get; set; }

        [Display(Name = "Phương thức đóng gói", GroupName = "Thông tin bổ sung")]
        public string PackingMethod { get; set; }

        // Stock info
        [Display(Name = "Quy tắc xuất", GroupName = "Thông tin kho")]
        public EnumStockOutputRule? StockOutputRuleId { get; set; }
        [Display(Name = "Cảnh báo số lượng tồn Min", GroupName = "Thông tin kho")]
        public long? AmountWarningMin { get; set; }
        [Display(Name = "Cảnh báo số lượng tồn Max", GroupName = "Thông tin kho")]
        public long? AmountWarningMax { get; set; }
        [Display(Name = "Hạn sử dụng", GroupName = "Thông tin kho")]
        public double? ExpireTimeAmount { get; set; }
        [Display(Name = "Đơn vị hạn sử dụng", GroupName = "Thông tin kho")]
        public EnumTimeType? ExpireTimeTypeId { get; set; }
        [Display(Name = "Danh sách kho", GroupName = "Thông tin kho")]
        public ICollection<int> StockIds { get; set; }

        [Display(Name = "ĐVT1 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public string SecondaryUnit01 { get; set; }
        [Display(Name = "ĐVT1 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression01 { get; set; }

        [Display(Name = "ĐVT2 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public string SecondaryUnit02 { get; set; }
        [Display(Name = "ĐVT2 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression02 { get; set; }

        [Display(Name = "ĐVT3 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public string SecondaryUnit03 { get; set; }
        [Display(Name = "ĐVT3 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression03 { get; set; }

        [Display(Name = "ĐVT4 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public string SecondaryUnit04 { get; set; }
        [Display(Name = "ĐVT4 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression04 { get; set; }

        [Display(Name = "ĐVT5 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public string SecondaryUnit05 { get; set; }
        [Display(Name = "ĐVT5 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression05 { get; set; }


        public ProductImportModel()
        {
            StockIds = new List<int>();
        }
    }
}
