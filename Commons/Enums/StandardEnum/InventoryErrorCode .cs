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
        InventoryCodeEmpty = 2,
        InventoryAlreadyExisted = 3,
        [Description("Kiện hàng không hợp lệ (sản phẩm hoặc đơn vị lưu không trùng khớp)")]
        InvalidPackage = 4,
        [Description("Phiếu đã được duyệt")]
        InventoryAlreadyApproved = 5,
        [Description("Mã phiếu đã tồn tại")]
        InventoryCodeAlreadyExisted = 6,
        [Description("Kiện không đủ số lượng để xuất kho")]
        NotEnoughQualtity = 7
    }

    public enum InventoryDetailErrorCode
    {
        InventoryDetailNotFound = 1,
        InventoryDetailCodeEmpty = 2,
        InventoryDetailAlreadyExisted = 3,
        OutOfStock = 4,
    }
}
