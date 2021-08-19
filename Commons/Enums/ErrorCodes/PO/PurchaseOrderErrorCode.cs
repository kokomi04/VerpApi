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

        [Description("Mặt hàng không có liên kết với đơn yêu cầu gia công")]
        NotExistsOutsourceRequestId = 6,

        [Description("Số lượng mặt hàng lớn hơn số lượng yêu cầu của gia công")]
        PrimaryQuanityGreaterThanQuantityRequirment = 7,
        
        [Description("Không tìm thấy vật tư du thừa")]
        ExcessNotFound = 8,
    }
}
