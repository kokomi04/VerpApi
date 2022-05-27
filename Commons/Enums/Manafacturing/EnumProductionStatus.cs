using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionStatus : int
    {
        /*
        [Description("Đang thiết lập")]
        NotReady = 9,
        [Description("Chờ sản xuât")]
        Waiting = 1,
        [Description("Đang sản xuất")]
        Processing = 2,
        [Description("Hoàn thành")]
        Finished = 3,
        [Description("Chậm tiến độ")]
        OverDeadline = 4*/

        /// <summary>
        /// Lệnh đang được tạo lập, chưa đầy đủ thông tin
        /// </summary>
        [Description("Chờ thiết lập")]
        NotReady = 9,

        /// <summary>
        /// Những lệnh chưa có bất cứ sự bàn giao nào trong quy trình
        /// </summary>
        [Description("Chờ sản xuât")]
        Waiting = 100,

        /// <summary>
        /// Đã có phát sinh số liệu thống kê SX hoặc bàn giao ở 1 hoặc 1 số công đoạn - Nhận đủ vật tư đầu vào
        /// </summary>
        [Description("Đang sản xuất")]
        ProcessingFullStarted = 200,

        /// <summary>
        ///  Đã có phát sinh số liệu thống kê SX hoặc bàn giao ở 1 hoặc 1 số công đoạn - Chưa nhận đủ vật tư đầu vào (cái này nhằm phân biệt các lệnh đang có vật tư chưa phân bổ)
        /// </summary>
        [Description("Đang sản xuất thiếu vật tư")]
        ProcessingLessStarted = 210,      

        /// <summary>
        /// Những lệnh đã quá ngày hoàn thành nhưng chưa có số liệu thống kê ở khâu cuối ra thành phẩm hoặc chưa có phiếu nhập kho thành phẩm
        /// </summary>
        [Description("Chậm tiến độ")]
        OverDeadline = 300,

        /// <summary>
        /// Là những lệnh đã hoàn tất các số liệu thống kê và nhập kho đầy đủ
        /// </summary>
        [Description("Hoàn thành")]
        Completed = 400,


        /// <summary>
        /// Là lệnh đã có số nhập kho đủ nhưng chưa có đầy đủ số liệu thống kê ở các công đoạn
        /// </summary>
        [Description("Thiếu dữ liệu thống kê")]
        MissHandOverInfo = 320,

        /// <summary>
        /// Là những lệnh đã xong nhưng số lượng không đủ như yêu cầu- Trạng thái này tương đương trạng thái hoàn thành và được thiết lập bằng tay. Các báo cáo ghi nhận lệnh này như là LSX đã hoàn thành
        /// </summary>
        [Description("Kết thúc")]
        Finished = 350

    }
}
