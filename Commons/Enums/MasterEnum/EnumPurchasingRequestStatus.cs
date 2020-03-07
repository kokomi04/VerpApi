using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    /// <summary>
    /// Kho - Phiếu nhập xuất
    /// </summary>
    public enum EnumPurchasingRequestStatus
    {
        /// <summary>
        /// Nháp 
        /// </summary>
        Editing = 0,

        /// <summary>
        /// Gửi - đợi duyệt
        /// </summary>
        WaitingApproved = 1,

        /// <summary>
        /// Từ chối
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Đã được duyệt
        /// </summary>
        Approved = 3,
    }
}
