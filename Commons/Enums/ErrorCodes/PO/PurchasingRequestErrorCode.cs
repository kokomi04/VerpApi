using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Net;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingRequestErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy phiếu yêu cầu VTHH tương ứng")]
        NotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã phiếu yêu cầu VTHH không có")]
        PurchasingRequestCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Phiếu yêu cầu VTHH đã tồn tại")]
        CodeAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thế xóa phiếu yêu cầu VTHH đã được duyệt")]
        AlreadyApproved = 4,
    }
}
