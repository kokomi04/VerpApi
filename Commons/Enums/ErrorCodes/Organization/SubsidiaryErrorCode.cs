using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes.Organization
{
    [ErrorCodePrefix("Subsidiary")]
    public enum SubsidiaryErrorCode
    {
        [Description("Công ty con/chi nhánh không tồn tại")]
        SubsidiaryNotfound = 1,
        [Description("Mã đã tồn tại")]
        SubsidiaryCodeExisted = 2,
        [Description("Tên đã tồn tại")]
        SubsidiaryNameExisted = 3,
    }
}
