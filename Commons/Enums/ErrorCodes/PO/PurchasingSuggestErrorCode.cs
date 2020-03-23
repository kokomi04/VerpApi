using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using VErp.Commons.Enums.StandardEnum;
using System.Net;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingSuggestErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy phiếu đề nghị mua hàng tương ứng")]
        SuggestNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã phiếu đề nghị mua hàng không có")]
        SuggestCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Phiếu đề nghị mua hàng đã tồn tại")]
        SuggestCodeAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thế xóa phiếu đề nghị mua hàng đã được duyệt")]
        SuggestAlreadyApproved = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã phân công đã tồn tại")]
        PoAssignmentCodeAlreadyExisted = 5,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy sản phẩm trong đề nghị mua hàng")]
        SuggestDetailNotfound = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Phân công vượt mức đề nghị mua")]
        PoAssignmentOverload = 6,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy phân công mua hàng")]
        PoAssignmentNotfound = 7,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Vui lòng xóa phân công mua hàng của đơn đề nghị này trước khi xóa!")]
        PoAssignmentNotEmpty = 8,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Vui lòng xóa phân công mua hàng của mặt hàng với mặt hàng xóa!")]
        PoAssignmentDetailNotEmpty = 9,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Vui lòng xóa PO liên quan đến mặt hàng phân công mua bị xóa!")]
        PurchaseOrderDetailNotEmpty = 10,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đề nghị mua hàng đang được phân công không được từ chối!")]
        CanNotRejectSuggestInUse = 11,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đề nghị mua hàng chưa được duyệt!")]
        PurchasingSuggestIsNotApprovedYet = 12,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Bạn chỉ được xác nhận phân công của mình!")]
        PoAssignmentConfirmInvalidCurrentUser = 13,
    }
}
