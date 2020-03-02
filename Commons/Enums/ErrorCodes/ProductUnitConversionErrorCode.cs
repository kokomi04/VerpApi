using System.ComponentModel;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CONV")]
    public enum ProductUnitConversionErrorCode
    {
        [Description("Đơn vị chuyển đổi không tìm thấy")]
        ProductUnitConversionNotFound = 1,
        [Description("Đơn vị chuyển đổi đã tồn tại")]
        ProductSecondaryUnitAlreadyExisted = 2,
        [Description("Không thể tính giá trị đơn vị chuyển đổi")]
        SecondaryUnitConversionError = 3,
        [Description("Không thể tính giá trị đơn vị chính")]
        PrimaryUnitConversionError = 4,
        [Description("Đơn vị chuyển đổi không thuộc về sản phẩm")]
        ProductUnitConversionNotBelongToProduct = 5
    }
}
