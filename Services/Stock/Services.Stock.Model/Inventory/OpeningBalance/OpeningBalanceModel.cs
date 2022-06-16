using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class OpeningBalanceModel
    {
        [Display(Name = "Loại", GroupName = "TT chung", Order = 1)]
        public EnumInventoryAction? InventoryActionId { get; set; }

        [Display(Name = "Mã phiếu", GroupName = "TT chung", Order = 2)]
        public string InventoryCode { get; set; }

        [Display(Name = "Tên kho", GroupName = "TT chung", Order = 3)]
        public int StockId { get; set; }

        [Display(Name = "Ngày", GroupName = "TT chung", Order = 4)]
        public long Date { get; set; }
        [Display(Name = "Mô tả", GroupName = "TT chung", Order = 5)]
        public string Description { get; set; }

        [Display(Name = "Người giao nhận", GroupName = "TT chung", Order = 6)]
        public string Shipper { get; set; }

        [Display(Name = "Khách hàng", GroupName = "TT chung", Order = 7)]
        [FieldDataNestedObject]
        public InvCustomerInfo Customer { get; set; }

        [Display(Name = "Bộ phận", GroupName = "TT chung", Order = 8)]
        [FieldDataNestedObject]
        public InvDepartmentInfo Department { get; set; }


        [Display(Name = "Mẫu hóa đơn", GroupName = "TT hóa đơn bổ sung", Order = 9)]
        public string BillForm { get; set; }

        [Display(Name = "Mã hóa đơn", GroupName = "TT hóa đơn bổ sung", Order = 10)]
        public string BillCode { get; set; }

        [Display(Name = "Serial hóa đơn", GroupName = "TT hóa đơn bổ sung", Order = 11)]
        public string BillSerial { get; set; }
        [Display(Name = "Ngày hóa đơn", GroupName = "TT hóa đơn bổ sung", Order = 12)]
        public long BillDate { get; set; }



        [Display(Name = "Danh mục mặt hàng", GroupName = "Sản phẩm", Order = 13)]
        public string CateName { set; get; }

        [Display(Name = "Loại mã mặt hàng", GroupName = "Sản phẩm", Order = 14)]
        public string CatePrefixCode { set; get; }

        [Display(Name = "Mã mặt hàng", GroupName = "Sản phẩm", Order = 15)]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng", GroupName = "Sản phẩm", Order = 16)]
        public string ProductName { set; get; }

        [Display(Name = "Chiều cao/dày mặt hàng", GroupName = "Sản phẩm", Order = 17)]
        public decimal Height { set; get; }

        [Display(Name = "Chiều rộng mặt hàng", GroupName = "Sản phẩm", Order = 18)]
        public decimal Width { set; get; }

        [Display(Name = "Chiều dài mặt hàng", GroupName = "Sản phẩm", Order = 19)]
        public decimal Long { set; get; }

        [Display(Name = "Quy cách", GroupName = "Sản phẩm", Order = 20)]
        public string Specification { set; get; }

        [Display(Name = "Đơn vị tính", GroupName = "Sản phẩm", Order = 21)]
        public string Unit1 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)", GroupName = "Thẻ Kho", Order = 22)]
        public decimal Qty1 { set; get; }

        [Display(Name = "Giá (Đơn vị chính)", GroupName = "Thẻ Kho", Order = 23)]
        public decimal UnitPrice { set; get; }


        [Display(Name = "Đơn vị chuyển đổi", GroupName = "Thẻ Kho", Order = 24)]
        public string Unit2 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho", Order = 25)]
        public decimal Qty2 { set; get; }

        [Display(Name = "Tỷ lệ Đơn vị chuyển đổi", GroupName = "Thẻ Kho", Order = 26)]
        public decimal Factor { set; get; }
    }


    public class ImportInvInputModel : OpeningBalanceModel
    {


        public static IList<EnumInventoryAction> InventoryActionIds = new[]
             {
                EnumInventoryAction.Normal,
                EnumInventoryAction.InputOfProduct,
                EnumInventoryAction.InputOfMaterial,
            };

        [Display(Name = "Thông tin kiện", GroupName = "Thông tin kiện", Order = 27)]
        [FieldDataNestedObject]
        public PackageInputModel ToPackgeInfo { get; set; }
    }

    public class ImportInvOutputModel : OpeningBalanceModel
    {
        public static IList<EnumInventoryAction> InventoryActionIds = new[]
          {
                EnumInventoryAction.Normal,
                EnumInventoryAction.OutputForSell,
                EnumInventoryAction.OutputForManufacture,
            };

        //[FieldDataType((int)EnumInventoryType.Output)]
        [Display(Name = "Kiện xuất", GroupName = "Thẻ Kho", Order = 27)]
        public string FromPackageCode { set; get; }
    }

    public class InvCustomerInfo
    {
        [FieldDataIgnore]
        public int? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }

    public class InvDepartmentInfo
    {
        [FieldDataIgnore]
        public int? DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
    }
}
