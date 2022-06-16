﻿using System.ComponentModel;

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


        [Description("Tạo kiện mới chung")]
        CreateMerge = 3,
    }
}
