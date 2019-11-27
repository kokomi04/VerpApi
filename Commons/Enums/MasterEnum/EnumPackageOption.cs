using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumPackageOption
    {
        /// <summary>
        /// Không phân kiện
        /// </summary>
        NoPackageManager = 0,
        /// <summary>
        /// Tạo kiện mới
        /// </summary>
        Create = 1,
        /// <summary>
        /// Thêm vào kiện đang có
        /// </summary>
        Append = 2,
    }
}
