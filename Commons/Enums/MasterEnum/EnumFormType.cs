using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumFormType
    {
        [Description("Nhập liệu")]
        Input = 1,
        [Description("Chọn từ danh sách")]
        Select = 2,
        [Description("Mã tự sinh")]
        Generate = 3,
        [Description("Bảng tìm kiếm")]
        SearchTable = 4,
        [Description("Nhập file")]
        ImportFile = 5,
        [Description("Chỉ hiển thị")]
        ViewOnly = 6,
        [Description("Chọn từ danh sách (chọn nhiều giá trị)")]
        MultiSelect = 7,
        [Description("Dynamic control")]
        DynamicControl = 8,
        [Description("Chỉ đọc")]
        ReadOnly = 9,
    }
}