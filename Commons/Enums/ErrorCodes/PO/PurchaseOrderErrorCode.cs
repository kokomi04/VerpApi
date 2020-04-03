using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes.PO
{
    [ErrorCodePrefix("PO")]
    public enum PurchaseOrderErrorCode
    {
        [Description("Không tìm thấy PO")]
        PoNotFound = 1,

        [Description("Mã phiếu PO đã tồn tại")]
        PoCodeAlreadyExisted = 2,

        [Description("Bạn đã tạo PO cho mặt hàng này rồi")]
        AssignmentDetailAlreadyCreatedPo = 3,

        [Description("Bạn chỉ có thể tạo PO cho 1 khách hàng")]
        OnlyCreatePOFromOneCustomer = 4,

        [Description("Bạn chỉ có thể tạo PO từ phân công mua hàng hoặc từ đề nghị mua hàng, không thể cả 2")]
        CreatePOFromOneOfPurchasingSuggestOrPoAssignmentOnly= 5,
    }
}
