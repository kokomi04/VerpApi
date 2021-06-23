using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Product.Partial
{
    public class ProductPartialGeneralModel
    {
        public int? ProductTypeId { get; set; }

        [Required(ErrorMessage ="Vui lòng nhập mã mặt hàng")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mặt hàng")]
        public string ProductName { get; set; }

        public string ProductNameEng { get; set; }

        public long? MainImageFileId { get; set; }

        public decimal? Long { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public int UnitId { get; set; }

        public int ProductCateId { get; set; }

        public double? ExpireTimeAmount { get; set; }
        public EnumTimeType? ExpireTimeTypeId { get; set; }

        public string Description { get; set; }


        public int? BarcodeConfigId { get; set; }
        public EnumBarcodeStandard? BarcodeStandardId { get; set; }
        public string Barcode { get; set; }

        public decimal? Quantitative { get; set; }
        public EnumQuantitativeUnitType? QuantitativeUnitTypeId { get; set; }

        public string Specification { get; set; }

        public decimal? EstimatePrice { get; set; }

        public bool IsProductSemi { get; set; }
        public bool? IsProduct { get; set; }
        public bool? IsMaterials { get; set; }
    }
}
