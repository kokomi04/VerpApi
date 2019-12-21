using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    public enum InventoryErrorCode
    {
        [Description("Không tìm thấy phiếu xuất/nhập kho")]
        InventoryNotFound = 1,
        [Description("Mã phiếu không có")]
        InventoryCodeEmpty = 2,
        [Description("Thông tin phiếu đã tồn tại")]
        InventoryAlreadyExisted = 3,
        [Description("Kiện hàng không hợp lệ (sản phẩm hoặc đơn vị lưu không trùng khớp)")]
        InvalidPackage = 4,
        [Description("Phiếu đã được duyệt")]
        InventoryAlreadyApproved = 5,
        [Description("Mã phiếu đã tồn tại")]
        InventoryCodeAlreadyExisted = 6,
        [Description("Kiện không đủ số lượng để xuất kho")]
        NotEnoughQuantity = 7,
        [Description("Tính năng này chưa được hỗ trợ")]
        NotSupportedYet = 8,

        [Description("Bạn cần cập nhật dữ liệu hợp lệ cho kiện/phiếu xuất (đầu ra <= đầu vào)")]
        InOuputAffectObjectsInvalid = 9,
    }

    public enum InventoryDetailErrorCode
    {
        [Description("Không tìm thấy thông tin chi tiết phiếu xuất/nhập kho")]
        InventoryDetailNotFound = 1,
        InventoryDetailCodeEmpty = 2,
        InventoryDetailAlreadyExisted = 3,
        OutOfStock = 4,
    }
}
