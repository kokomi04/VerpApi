using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("OUTS")]
    public enum OutsourceErrorCode
    {
        [Description("Không tìm thấy yêu cầu gia công")]
        NotFoundRequest = 1,
        [Description("Không tìm thấy đơn hàng gia công")]
        NotFoundOutsourceOrder = 2,
        InValidRequestOutsource = 3,
        [Description("Đã tồn tại mã code trong hệ thống")]
        OutsoureOrderCodeAlreadyExisted = 4,
        [Description("Không tìm thấy yêu cầu gia công chi tiết")]
        NotFoundOutsourOrderDetail = 5,
        [Description("Đã tồn tại công đoạn có yêu cầu gia công")]
        EarlyExistsProductionStepHadOutsourceRequest = 6,
    }
}
