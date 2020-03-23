using System.ComponentModel;
using System.Net;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PAK")]
    public enum PackageErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy kiện tương ứng")]
        PackageNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã kiện không có")]
        PackageCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện đã tồn tại")]
        PackageAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện này không cho phép cập nhật")]
        PackageNotAllowUpdate = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Số lượng hàng trong kiện không đủ")]
        QualtityOfProductInPackageNotEnough = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện hàng đang tồn tại một lượng chờ duyệt")]
        HasSomeQualtityWaitingForApproved = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Các kiện được gộp phải cùng kho chứa, mặt hàng và đơn vị tính (cùng đơn vị chuyển đổi)")]
        PackagesToJoinMustBeSameProductAndUnit = 7,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện mặc định không được phép gộp")]
        CanNotJoinDefaultPackage = 8
    }
}
