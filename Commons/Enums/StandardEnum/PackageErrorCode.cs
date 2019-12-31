using System.ComponentModel;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    public enum PackageErrorCode
    {
        [Description("Không tìm thấy kiện tương ứng")]
        PackageNotFound = 1,
        [Description("Mã kiện không có")]
        PackageCodeEmpty = 2,
        [Description("Kiện đã tồn tại")]
        PackageAlreadyExisted = 3,
        [Description("Kiện này không cho phép cập nhật")]
        PackageNotAllowUpdate = 4,
        [Description("Số lượng hàng trong kiện không đủ")]
        QualtityOfProductInPackageNotEnough = 5,
        [Description("Kiện hàng đang tồn tại một lượng chờ duyệt")]
        HasSomeQualtityWaitingForApproved = 6,
        [Description("Các kiện được gộp phải cùng kho chứa, mặt hàng và đơn vị tính (cùng đơn vị chuyển đổi)")]
        PackagesToJoinMustBeSameProductAndUnit = 7,
        [Description("Kiện mặc định không được phép gộp")]
        CanNotJoinDefaultPackage = 8
    }
}
