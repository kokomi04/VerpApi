using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOutExcelParseModel
    {

        [Display(Name = "Mã mặt hàng", GroupName = "Sản phẩm")]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng", GroupName = "Sản phẩm")]
        public string ProductName { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal? PrimaryQuantity { set; get; }

        [Display(Name = "Giá (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal? UnitPrice { set; get; }

        [Display(Name = "Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
        public string ProductUnitConversionName { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal? ProductUnitConversionQuantity { set; get; }

        [Display(Name = "Giá (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal? ProductUnitConversionPrice { set; get; }

        //[Display(Name = "Tài khoản kế toán đối ứng", GroupName = "Thẻ Kho")]
        //public string AccountancyAccountNumberDu { set; get; }

        [Display(Name = "Mã PO", GroupName = "TT Bổ sung")]
        public string PoCode { set; get; }

        [Display(Name = "Mã đơn hàng", GroupName = "TT Bổ sung")]
        public string OrderCode { set; get; }

        [Display(Name = "Mã LSX", GroupName = "TT Bổ sung")]
        public string ProductionOrderCode { set; get; }

        [Display(Name = "Mô tả", GroupName = "TT Bổ sung")]
        public string Description { set; get; }


        [Display(Name = "Kiện xuất (Nếu có) - Mã kiện", GroupName = "Thẻ Kho")]
        //[FieldDataType((int)EnumInventoryType.Output)]
        public string FromPackageCode { set; get; }
    }


    public class InventoryInputExcelParseModel
    {
        public InventoryInputExcelParseModel()
        {
            ToPackgeInfo = new PackageInputModel();
        }

        [Display(Name = "Mã mặt hàng", GroupName = "Sản phẩm")]
        public string ProductCode { set; get; }

        [Display(Name = "Tên mặt hàng", GroupName = "Sản phẩm")]
        public string ProductName { set; get; }

        [Display(Name = "Số lượng (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal? PrimaryQuantity { set; get; }

        [Display(Name = "Giá (Đơn vị chính)", GroupName = "Thẻ Kho")]
        public decimal? UnitPrice { set; get; }

        [Display(Name = "Đơn vị chuyển đổi", GroupName = "Thẻ Kho")]
        public string ProductUnitConversionName { set; get; }

        [Display(Name = "Số lượng (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal? ProductUnitConversionQuantity { set; get; }

        [Display(Name = "Giá (Đơn vị chuyển đổi)", GroupName = "Thẻ Kho")]
        public decimal? ProductUnitConversionPrice { set; get; }

        [Display(Name = "Mã PO", GroupName = "TT Bổ sung")]
        public string PoCode { set; get; }

        [Display(Name = "Mã đơn hàng", GroupName = "TT Bổ sung")]
        public string OrderCode { set; get; }

        [Display(Name = "Mã LSX", GroupName = "TT Bổ sung")]
        public string ProductionOrderCode { set; get; }

        [Display(Name = "Mô tả", GroupName = "TT Bổ sung")]
        public string Description { set; get; }


        [Display(Name = "Tạo kiện mới (Có: Tạo kiện mới với thông tin kiện, Không: Nhập vào kiện mặc định hoặc kiện có mã đang có)", GroupName = "TT Bổ sung")]
        //[FieldDataType((int)EnumInventoryType.Input)]
        public EnumPackageOption PackageOptionId { set; get; }


        [Display(Name = "Thông tin kiện", GroupName = "Thông tin kiện")]
        [FieldDataNestedObject]
        public PackageInputModel ToPackgeInfo { get; set; }
    }


    public class InvInputDetailRowValue
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }


        public int PrimaryUnitId { get; set; }
        public string PrimaryUnitName { get; set; }
        public decimal? PrimaryQuantity { get; set; }
        public decimal? PrimaryPrice { get; set; }

        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public string ProductUnitConversionExpression { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }

        public string PoCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public EnumPackageOption PackageOptionId { get; set; }
        public long? ToPackageId { get; set; }
        public string ToPackageCode { get; set; }

        public PackageInputModel ToPackageInfo { get; set; }
    }


    public class InvOutDetailRowValue
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }


        public int PrimaryUnitId { get; set; }
        public string PrimaryUnitName { get; set; }
        public decimal? PrimaryQuantity { get; set; }
        public decimal? PrimaryPrice { get; set; }

        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public string ProductUnitConversionExpression { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }

        public string PoCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public long? FromPackageId { get; set; }
        public string FromPackageCode { get; set; }
        public InventoryDetailRowPackage PackageInfo { get; set; }
        public class InventoryDetailRowPackage
        {
            public long PackageId { get; set; }
            public string PackageCode { get; set; }
            public decimal PrimaryQuantityRemaining { get; set; }
            public decimal ProductUnitConversionRemaining { get; set; }
        }
    }
}
