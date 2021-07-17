using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Product
{
    /// <summary>
    /// model to parse from excel
    /// <b>NOTE: All props need to be nullable for check null to append prop to existed product</b>
    /// </summary>
    [Display(Name = "Sản phẩm")]
    public class ProductImportModel : MappingDataRowAbstract
    {

        // General info
        [Display(Name = "Mã mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng nhập mã mặt hàng")]
        [MaxLength(128, ErrorMessage = "Mã mặt hàng quá dài")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên mặt hàng", GroupName = "Thông tin chung")]
        [MaxLength(128, ErrorMessage = "Tên mặt hàng quá dài")]
        public string ProductName { get; set; }

        [Display(Name = "Mã loại", GroupName = "Thông tin chung")]
        public string ProductTypeCode { get; set; }

        [Display(Name = "Tên loại mã", GroupName = "Thông tin chung")]
        public string ProductTypeName { get; set; }

        [Display(Name = "Danh mục mặt hàng", GroupName = "Thông tin chung")]
        public string ProductCate { get; set; }

        [Display(Name = "Cấu hình Barcode", GroupName = "Thông tin chung")]
        public int? BarcodeConfigId { get; set; }

        [Display(Name = "Barcode", GroupName = "Thông tin chung")]
        public string Barcode { get; set; }



        [Display(Name = "Tên Đơn vị chính", GroupName = "Đơn vị chính")]
        public string Unit { get; set; }

        [Display(Name = "Độ chính xác (đơn vị chính)", GroupName = "Đơn vị chính")]
        public int? DecimalPlaceDefault { get; set; }

        [Display(Name = "Giá ước tính", GroupName = "TT Mua bán")]
        public decimal? EstimatePrice { get; set; }

        [Display(Name = "Là chi tiết mặt hàng (Có, Không)", GroupName = "Thông tin chung")]
        public bool? IsProductSemi { get; set; }

        [Display(Name = "Là thành phẩm (Có, Không)", GroupName = "Thông tin chung")]
        public bool? IsProduct { get; set; }

        [Display(Name = "Là NVL (Có, Không)", GroupName = "Thông tin chung")]
        public bool? IsMaterials { get; set; }

        [Display(Name = "Cơ số sản phẩm", GroupName = "TT Sản xuất")]
        public int? Coefficient { get; set; }



        [FieldDataIgnore]
        public int? CustomerId { get; set; }

        [Display(Name = "Mã Khách hàng", GroupName = "TT Khách hàng")]
        public string CustomerCode { get; set; }

        [Display(Name = "Tên Khách hàng", GroupName = "TT Khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Mã lưu bên k.hàng", GroupName = "TT Khách hàng")]
        public string CustomerProductCode { get; set; }

        [Display(Name = "Tên lưu bên k.hàng", GroupName = "TT Khách hàng")]
        public string CustomerProductName { get; set; }



        // Extra info
        [Display(Name = "Quy cách", GroupName = "Thông tin bổ sung")]
        public string Specification { get; set; }

        [Display(Name = "Ghi chú", GroupName = "Thông tin bổ sung")]
        public string Description { get; set; }

        [Display(Name = "Phương thức đóng gói", GroupName = "Thông tin bổ sung")]
        public string PackingMethod { get; set; }




        [Display(Name = "Định lượng", GroupName = "Thông số")]
        public decimal? Quantitative { get; set; }

        [Display(Name = "Đơn vị Định lượng(g/m2, g/m3)", GroupName = "Thông số")]
        public EnumQuantitativeUnitType? QuantitativeUnitTypeId { get; set; }

        [Display(Name = "Dài (mm)", GroupName = "Thông số")]
        public decimal? Long { get; set; }

        [Display(Name = "Rộng (mm)", GroupName = "Thông số")]
        public decimal? Width { get; set; }

        [Display(Name = "Cao (mm)", GroupName = "Thông số")]
        public decimal? Height { get; set; }

        [Display(Name = "Thể tích (m3)", GroupName = "Thông số")]
        public decimal? Measurement { get; set; }

        [Display(Name = "Trọng lượng tịnh (g)", GroupName = "Thông số")]
        public decimal? NetWeight { get; set; }

        [Display(Name = "Tổng trọng lượng (g)", GroupName = "Thông số")]
        public decimal? GrossWeight { get; set; }

        [Display(Name = "Tải trọng (g)", GroupName = "Thông số")]
        public decimal? LoadAbility { get; set; }


        // Stock info
        [Display(Name = "Quy tắc xuất", GroupName = "TT lưu kho")]
        public EnumStockOutputRule? StockOutputRuleId { get; set; }

        [Display(Name = "Cảnh báo số lượng tồn Min", GroupName = "TT lưu kho")]
        public long? AmountWarningMin { get; set; }

        [Display(Name = "Cảnh báo số lượng tồn Max", GroupName = "TT lưu kho")]
        public long? AmountWarningMax { get; set; }

        [Display(Name = "Hạn sử dụng", GroupName = "TT lưu kho")]
        public double? ExpireTimeAmount { get; set; }

        [Display(Name = "Đơn vị hạn sử dụng", GroupName = "TT lưu kho")]
        public EnumTimeType? ExpireTimeTypeId { get; set; }

        [Display(Name = "Danh sách kho chứa mặc định", GroupName = "TT lưu kho")]
        public ICollection<int> StockIds { get; set; }

        // UnitConversion       


        [Display(Name = "ĐVT2 - Tên", GroupName = "Đơn vị chuyển đổi 2")]
        public string SecondaryUnit02 { get; set; }

        [Display(Name = "ĐVT2 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi 2")]
        public string FactorExpression02 { get; set; }

        [Display(Name = "ĐVT2 - Độ chính xác", GroupName = "Đơn vị chuyển đổi 2")]
        public int? DecimalPlace02 { get; set; }



        [Display(Name = "ĐVT3 - Tên", GroupName = "Đơn vị chuyển đổi 3")]
        public string SecondaryUnit03 { get; set; }

        [Display(Name = "ĐVT3 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi 3")]
        public string FactorExpression03 { get; set; }

        [Display(Name = "ĐVT3 - Độ chính xác", GroupName = "Đơn vị chuyển đổi 3")]
        public int? DecimalPlace03 { get; set; }



        [Display(Name = "ĐVT4 - Tên", GroupName = "Đơn vị chuyển đổi 4")]
        public string SecondaryUnit04 { get; set; }

        [Display(Name = "ĐVT4 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi 4")]
        public string FactorExpression04 { get; set; }

        [Display(Name = "ĐVT4 - Độ chính xác", GroupName = "Đơn vị chuyển đổi 4")]
        public int? DecimalPlace04 { get; set; }



        [Display(Name = "ĐVT5 - Tên", GroupName = "Đơn vị chuyển đổi 5")]
        public string SecondaryUnit05 { get; set; }

        [Display(Name = "ĐVT5 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi 5")]
        public string FactorExpression05 { get; set; }

        [Display(Name = "ĐVT5 - Độ chính xác", GroupName = "Đơn vị chuyển đổi 5")]
        public int? DecimalPlace05 { get; set; }


        public ProductImportModel()
        {
            StockIds = new List<int>();
        }
    }
}
