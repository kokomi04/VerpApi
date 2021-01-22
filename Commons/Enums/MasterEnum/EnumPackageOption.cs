using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumPackageOption
    {
        /// <summary>
        /// Không phân kiện
        /// </summary>
        [Description("Không phân kiện")]
        NoPackageManager = 0,
        /// <summary>
        /// Tạo kiện mới
        /// </summary>
        [Description("Tạo kiện mới")]
        Create = 1,
        /// <summary>
        /// Thêm vào kiện đang có
        /// </summary>
        [Description("Thêm vào kiện đang có")]
        Append = 2,
    }
}
