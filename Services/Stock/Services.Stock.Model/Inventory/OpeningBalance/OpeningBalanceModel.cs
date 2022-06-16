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
        [Display(Name = "Loại", GroupName = "TT chung")]
        public EnumInventoryAction? InventoryActionId { get; set; }

        [Display(Name = "Mã phiếu", GroupName = "TT chung")]
        public string InventoryCode { get; set; }

        [Display(Name = "Tên kho", GroupName = "TT chung")]
        public int StockId { get; set; }

        [Display(Name = "Ngày", GroupName = "TT chung")]
        public long Date { get; set; }
        [Display(Name = "Mô tả", GroupName = "TT chung")]
        public string Description { get; set; }

        [Display(Name = "Người giao nhận", GroupName = "TT chung")]
        public string Shipper { get; set; }

        [Display(Name = "Khách hàng", GroupName = "TT chung")]
        [FieldDataNestedObject]
        public InvCustomerInfo Customer { get; set; }

        [Display(Name = "Bộ phận", GroupName = "TT chung")]
        [FieldDataNestedObject]
        public InvDepartmentInfo Department { get; set; }


        [Display(Name = "Mẫu hóa đơn", GroupName = "TT hóa đơn bổ sung")]
        public string BillForm { get; set; }

        [Display(Name = "Mã hóa đơn", GroupName = "TT hóa đơn bổ sung")]
        public string BillCode { get; set; }

        [Display(Name = "Serial hóa đơn", GroupName = "TT hóa đơn bổ sung")]
        public string BillSerial { get; set; }
        [Display(Name = "Ngày hóa đơn", GroupName = "TT hóa đơn bổ sung")]
        public long BillDate { get; set; }



        [Display(Name = "Danh mục mặt hàng", GroupName = "Sản phẩm")]
        public string CateName { set; get; }

        [Display(Name = "Loại mã mặt hàng", GroupName = "Sản phẩm")]
        public string CatePrefixCode { set; get; }

        [Display(Name = "Mã mặt hàng", GroupName = "Sản phẩm")]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng", GroupName = "Sản phẩm")]
        public string ProductName { set; get; }

        [Display(Name = "Chiều cao/dày mặt hàng", GroupName = "Sản phẩm")]
        public decimal Height { set; get; }

        [Display(Name = "Chiều rộng mặt hàng", GroupName = "Sản phẩm")]
        public decimal Width { set; get; }

        [Display(Name = "Chiều dài mặt hàng", GroupName = "Sản phẩm")]
        public decimal Long { set; get; }

        [Display(Name = "Quy cách", GroupName = "Sản phẩm")]
        public string Specification { set; get; }

        [Display(Name = "Đơn vị tính", GroupName = "Sản phẩm")]
        public string Unit1 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal Qty1 { set; get; }

        [Display(Name = "Giá (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal UnitPrice { set; get; }


        [Display(Name = "Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
        public string Unit2 { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal Qty2 { set; get; }

        [Display(Name = "Tỷ lệ Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
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

        [Display(Name = "Thông tin kiện", GroupName = "Thông tin kiện")]
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
        [Display(Name = "Kiện xuất", GroupName = "Thẻ Kho")]
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
