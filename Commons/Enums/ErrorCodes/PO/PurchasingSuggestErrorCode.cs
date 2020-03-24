using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingSuggestErrorCode
    {
        [Description("Không tìm thấy phiếu đề nghị mua hàng tương ứng")]
        SuggestNotFound = 1,
        [Description("Mã phiếu đề nghị mua hàng không có")]
        SuggestCodeEmpty = 2,
        [Description("Phiếu đề nghị mua hàng đã tồn tại")]
        SuggestCodeAlreadyExisted = 3,
        [Description("Không thế xóa phiếu đề nghị mua hàng đã được duyệt")]
        SuggestAlreadyApproved = 4,
        [Description("Mã phân công đã tồn tại")]
        PoAssignmentCodeAlreadyExisted = 5,
        [Description("Không tìm thấy sản phẩm trong đề nghị mua hàng")]
        SuggestDetailNotfound = 6,
        [Description("Phân công vượt mức đề nghị mua")]
        PoAssignmentOverload = 6,
        [Description("Không tìm thấy phân công mua hàng")]
        PoAssignmentNotfound = 7,
        [Description("Vui lòng xóa phân công mua hàng của đơn đề nghị này trước khi xóa!")]
        PoAssignmentNotEmpty = 8,
        [Description("Vui lòng xóa phân công mua hàng của mặt hàng với mặt hàng xóa!")]
        PoAssignmentDetailNotEmpty = 9,
        [Description("Vui lòng xóa PO liên quan đến mặt hàng phân công mua bị xóa!")]
        PurchaseOrderDetailNotEmpty = 10,
        [Description("Đề nghị mua hàng đang được phân công không được từ chối!")]
        CanNotRejectSuggestInUse = 11,
        [Description("Đề nghị mua hàng chưa được duyệt!")]
        PurchasingSuggestIsNotApprovedYet = 12,
        [Description("Bạn chỉ được xác nhận phân công của mình!")]
        PoAssignmentConfirmInvalidCurrentUser = 13,
    }
}
