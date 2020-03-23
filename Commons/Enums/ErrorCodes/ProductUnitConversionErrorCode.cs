using System.ComponentModel;
using System.Net;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CONV")]
    public enum ProductUnitConversionErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Đơn vị chuyển đổi không tìm thấy")]
        ProductUnitConversionNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đơn vị chuyển đổi đã tồn tại")]
        ProductSecondaryUnitAlreadyExisted = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thể tính giá trị đơn vị chuyển đổi")]
        SecondaryUnitConversionError = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thể tính giá trị đơn vị chính")]
        PrimaryUnitConversionError = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đơn vị chuyển đổi không thuộc về sản phẩm")]
        ProductUnitConversionNotBelongToProduct = 5
    }
}
