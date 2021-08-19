using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PROD")]
    public enum ProductOrderErrorCode
    {
        [Description("Lệnh sản xuất không tồn tại")]
        ProductOrderNotfound = 1,
        [Description("Mã lệnh sản xuất đã tồn tại")]
        ProductOrderCodeAlreadyExisted = 2,
        NotFoundMaterials = 3
    }
}
