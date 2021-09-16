using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Library.Model;

namespace VErp.Services.PurchaseOrder.Model.Request
{
    //public class SingleInvoicePurchasingRequestExcelMappingModel
    //{
    //    public string SheetName { get; set; }
    //    public int FromRow { get; set; }
    //    public int ToRow { get; set; }

    //    public SingleInvoiceStaticContent StaticValue { get; set; }

    //    public PurchasingRequestDetailMappingColumn ColumnMapping{ get; set; }

     

    //    public class PurchasingRequestDetailMappingColumn
    //    {
    //        public string ProductCodeColumn { get; set; }
    //        public string ProductNameColumn { get; set; }
    //        public string PrimaryQuantityColumn { get; set; }            
    //        public string ProductUnitConversionNameColumn { get; set; }
    //        public string ProductUnitConversionQuantityColumn { get; set; }
    //        public string OrderCodeColumn { get; set; }
    //        public string ProductionOrderCodeColumn { get; set; }
    //        public string DescriptionColumn { get; set; }
    //    }
    //}

    public class SingleInvoiceStaticContent
    {
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }

        public string ProductUnitConversionName { get; set; }
    }

    [Display(Name = "Chi tiết Yêu cầu vật tư")]
    public class PurchasingRequestDetailRowValue: MappingDataRowAbstract
    {
        [Display(Name = "Mã mặt hàng", GroupName = "Mặt hàng")]
        public string ProductCode { get; set; }
        [Display(Name = "Tên mặt hàng", GroupName = "Mặt hàng")]
        public string ProductName { get; set; }

        [FieldDataIgnore]
        public string ProductInternalName { get; set; }
        [Display(Name = "Số lượng Đơn vị chính", GroupName = "Số lượng")]
        public decimal PrimaryQuantity { get; set; }
        [Display(Name = "Tên Đơn vị chuyển đổi", GroupName = "Số lượng")]
        public string ProductUnitConversionName { get; set; }
        [Display(Name = "Số lượng Đơn vị chuyển đổi", GroupName = "Số lượng")]

        public decimal ProductUnitConversionQuantity { get; set; }
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
