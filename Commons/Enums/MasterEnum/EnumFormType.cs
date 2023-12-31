﻿using System.ComponentModel;

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

        [Description("Tổ hợp nhiều dữ liệu tùy chọn - Custom area")]
        DynamicControl = 8,

        [Description("Sql Select")]
        SqlSelect = 9
    }
}