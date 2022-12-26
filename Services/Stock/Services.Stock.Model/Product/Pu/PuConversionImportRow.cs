using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Product.Pu
{

    [Display(Name = "Đơn vị chuyển đổi")]
    public class PuConversionImportRow : MappingDataRowAbstract
    {

        [Display(Name = "Mặt hàng", GroupName = "Mặt hàng", Order = 1001)]
        public PuConversionImportProductInfo ProductInfo { get; set; }

        [FieldDataIgnore]
        public int? ProductUnitConversionId { get; set; }

        [Display(Name = "Tên đơn vị chuyển đổi", GroupName = "Thông tin", Order = 2001)]
        public string ProductUnitConversionName { get; set; }

        [FieldDataIgnore]
        public string ProductUnitConversionInternalName { get; set; }

        [Display(Name = "Tỷ lệ/Biểu thức chuyển đổi", GroupName = "Thông tin", Order = 2002)]
        public string FactorExpression { get; set; }

        [Display(Name = "Độ chính xác", GroupName = "Thông tin", Order = 2003)]
        public int? DecimalPlace { get; set; }

        [Display(Name = "Mô tả", GroupName = "Thông tin", Order = 2004)]
        public string ConversionDescription { get; set; }

        [Display(Name = "Là đơn vị chính (Có/Không)", GroupName = "Thông tin", Order = 2005)]
        public bool IsDefault { get; set; }
//        public bool IsFreeStyle { get; set; }
    }

    public class PuConversionImportProductInfo
    {
        [FieldDataIgnore]
        public int ProductId { get; set; }

        [Display(Name = "Mã mặt hàng", GroupName = "Mặt hàng", Order = 1)]
        public string ProductCode { get; set; }
        [Display(Name = "Tên mặt hàng", GroupName = "Mặt hàng", Order = 2)]
        public string ProductName { get; set; }

    }



}
