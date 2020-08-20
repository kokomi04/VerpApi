using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductImportModel
    {
        // General info
        [Display(Name = "Mã mặt hàng", GroupName ="Thông tin chung")]
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
        //[Display(Name = "Ảnh mặt hàng")]
        //public long? MainImageFileId { get; set; }
        [Display(Name = "Loại mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn loại mặt hàng")]
        public int ProductTypeId { get; set; }
        [Display(Name = "Danh mục mặt hàng", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục mặt hàng")]
        public int ProductCateId { get; set; }
        [Display(Name = "Cấu hình Barcode", GroupName = "Thông tin chung")]
        public int? BarcodeConfigId { get; set; }
        [Display(Name = "Barcode", GroupName = "Thông tin chung")]
        public string Barcode { get; set; }
        [Display(Name = "Đơn vị chính", GroupName = "Thông tin chung")]
        [Required(ErrorMessage = "Vui lòng chọn đơn vị chính")]
        public int UnitId { get; set; }
        [Display(Name = "Giá ước tính", GroupName = "Thông tin chung")]
        public decimal? EstimatePrice { get; set; }

        // Extra info
        [Display(Name = "Quy cách", GroupName = "Thông tin bổ sung")]
        public string Specification { get; set; }
        //[Display(Name = "Mô tả")]
        //public string Description { get; set; }

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
        //[Display(Name = "Ghi chú kho")]
        //public string DescriptionToStock { get; set; }
        [Display(Name = "Danh sách kho", GroupName = "Thông tin kho")]
        public ICollection<int> StockIds { get; set; }

        // Unit conversion
        //[Display(Name = "Quy cách đơn vị chuyển đổi 01")]
        //[MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi 01 quá dài")]
        //public string ProductUnitConversionName01 { get; set; }D:\VERP\verp-api\Services\Stock\Services.Stock.Model\Inventory\
        [Display(Name = "ĐVT1 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public int? SecondaryUnitId01 { get; set; }
        [Display(Name = "ĐVT1 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression01 { get; set; }
        //[Display(Name = "Mô tả quy cách chuyển đổi 01")]
        //public string ConversionDescription01 { get; set; }

        //[Display(Name = "Quy cách đơn vị chuyển đổi 02")]
        //[MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi 02 quá dài")]
        //public string ProductUnitConversionName02 { get; set; }
        [Display(Name = "ĐVT2 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public int? SecondaryUnitId02 { get; set; }
        [Display(Name = "ĐVT2 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression02 { get; set; }
        //[Display(Name = "Mô tả quy cách chuyển đổi 02")]
        //public string ConversionDescription02 { get; set; }

        //[Display(Name = "Quy cách đơn vị chuyển đổi 03")]
        //[MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi 03 quá dài")]
        //public string ProductUnitConversionName03 { get; set; }
        [Display(Name = "ĐVT3 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public int? SecondaryUnitId03 { get; set; }
        [Display(Name = "ĐVT3 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression03 { get; set; }
        //[Display(Name = "Mô tả quy cách chuyển đổi 03")]
        //public string ConversionDescription03 { get; set; }

        //[Display(Name = "Quy cách đơn vị chuyển đổi 04")]
        //[MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi 04 quá dài")]
        //public string ProductUnitConversionName04 { get; set; }
        [Display(Name = "ĐVT4 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public int? SecondaryUnitId04 { get; set; }
        [Display(Name = "ĐVT4 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression04 { get; set; }
        //[Display(Name = "Mô tả quy cách chuyển đổi 04")]
        //public string ConversionDescription04 { get; set; }

        //[Display(Name = "Quy cách đơn vị chuyển đổi 05")]
        //[MaxLength(128, ErrorMessage = "Quy cách đơn vị chuyển đổi 05 quá dài")]
        //public string ProductUnitConversionName05 { get; set; }
        [Display(Name = "ĐVT5 - Tên", GroupName = "Đơn vị chuyển đổi")]
        public int? SecondaryUnitId05 { get; set; }
        [Display(Name = "ĐVT5 - Tỷ lệ", GroupName = "Đơn vị chuyển đổi")]
        public string FactorExpression05 { get; set; }
        //[Display(Name = "Mô tả quy cách chuyển đổi 05")]
        //public string ConversionDescription05 { get; set; }

        public ProductImportModel()
        {
            StockIds = new List<int>();
        }
    }
}
