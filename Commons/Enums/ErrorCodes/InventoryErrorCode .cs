using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    [ErrorCodePrefix("INV")]
    public enum InventoryErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy phiếu xuất/nhập kho")]
        InventoryNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã phiếu không có")]
        InventoryCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Thông tin phiếu đã tồn tại")]
        InventoryAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện hàng không hợp lệ (sản phẩm hoặc đơn vị lưu không trùng khớp)")]
        InvalidPackage = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Phiếu đã được duyệt")]
        InventoryAlreadyApproved = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã phiếu đã tồn tại")]
        InventoryCodeAlreadyExisted = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Kiện không đủ số lượng để xuất kho")]
        NotEnoughQuantity = 7,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tính năng này chưa được hỗ trợ")]
        NotSupportedYet = 8,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Bạn cần cập nhật dữ liệu hợp lệ cho kiện/phiếu xuất (đầu ra <= đầu vào)")]
        InOuputAffectObjectsInvalid = 9,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thể thay đổi kho ở phiếu nhập/xuất")]
        CanNotChangeStock = 10,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Phiếu chưa được duyệt")]
        InventoryNotApprovedYet = 11,
    }
}
