using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Library.Model;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    //public class SingleInvoicePoExcelMappingModel
    //{
    //    public string SheetName { get; set; }
    //    public int FromRow { get; set; }
    //    public int ToRow { get; set; }

    //    public SinglePoStaticContent StaticValue { get; set; }

    //    public PoDetailMappingColumn ColumnMapping { get; set; }

    //    public class SinglePoStaticContent
    //    {
    //        public string OrderCode { get; set; }
    //        public string ProductionOrderCode { get; set; }

    //        public string ProductUnitConversionName { get; set; }
    //    }

    //    public class PoDetailMappingColumn
    //    {
    //        public string ProductCodeColumn { get; set; }
    //        public string ProductNameColumn { get; set; }
    //        public string ProductProviderNameColumn { get; set; }

    //        public string PrimaryQuantityColumn { get; set; }
    //        public string PrimaryPriceColumn { get; set; }

    //        public string ProductUnitConversionNameColumn { get; set; }
    //        public string ProductUnitConversionQuantityColumn { get; set; }
    //        public string ProductUnitConversionPriceColumn { get; set; }

    //        public string MoneyColumn { get; set; }

    //        public string TaxInPercentColumn { get; set; }
    //        public string TaxInMoneyColumn { get; set; }

    //        public string OrderCodeColumn { get; set; }
    //        public string ProductionOrderCodeColumn { get; set; }
    //        public string DescriptionColumn { get; set; }
    //    }
    //}

    [Display(Name = "Chi tiết đơn mua hàng")]
    public class PoDetailRowValue
    {
        [Display(Name = "Mã mặt hàng", GroupName = "Mặt hàng")]
        public string ProductCode { get; set; }
        [Display(Name = "Tên mặt hàng", GroupName = "Mặt hàng")]
        public string ProductName { get; set; }

        [FieldDataIgnore]
        public string ProductInternalName { get; set; }

        [Display(Name = "Tên mặt hàng tương ứng NCC", GroupName = "Mặt hàng")]
        public string ProductProviderName { get; set; }

        [Display(Name = "Số lượng Đơn vị chính", GroupName = "TT về lượng")]
        public decimal? PrimaryQuantity { get; set; }

        [Display(Name = "Giá theo đơn vị chính", GroupName = "TT về lượng")]
        public decimal? PrimaryPrice { get; set; }

        [Display(Name = "Tên Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public string ProductUnitConversionName { get; set; }
        
        [Display(Name = "Số lượng Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public decimal? ProductUnitConversionQuantity { get; set; }
        [Display(Name = "Giá theo Đơn vị chuyển đổi", GroupName = "TT về lượng")]
        public decimal? ProductUnitConversionPrice { get; set; }
        [Display(Name = "Thành tiền ngoại tệ", GroupName = "TT về lượng")]
        public decimal? IntoMoney { get; set; }
        [Display(Name = "Thành tiền VNĐ", GroupName = "TT về lượng")]
        public decimal? ExchangedMoney { get; set; }


        //[Display(Name = "Thuế theo phần trăm", GroupName = "TT về lượng")]
        //public decimal TaxInPercent { get; set; }
        //[Display(Name = "Thuế theo tiền", GroupName = "TT về lượng")]
        //public decimal TaxInMoney { get; set; }

        [Display(Name = "Mã đơn hàng", GroupName = "Bổ sung")]
        public string OrderCode { get; set; }
        [Display(Name = "Mã LSX", GroupName = "Bổ sung")]
        public string ProductionOrderCode { get; set; }
        [Display(Name = "Mô tả", GroupName = "Bổ sung")]
        public string Description { get; set; }
        [Display(Name = "Thứ tự sắp xếp", GroupName = "Bổ sung")]
        public int? SortOrder { get; set; }


    }
}
