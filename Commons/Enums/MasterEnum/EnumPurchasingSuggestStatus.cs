using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    /// <summary>
    /// PurchasingSuggest Status - Trạng thái của phiếu đề nghị mua VT HH
    /// </summary>
    public enum EnumPurchasingSuggestStatus
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

        /// <summary>
        /// Đã gửi nhà cung cấp
        /// </summary>
        SendedToProvider = 4,
        
        /// <summary>
        /// Đã hoàn thành
        /// </summary>
        Completed = 5,
    }
}
