using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumDataType
    {
        [Description("Text")]
        [DataSize(512)]
        [Regex("")]
        Text = 1,
        [Description("Int")]
        [DataSize(-1)]
        [Regex("^[0-9]*$")]
        Int = 2,
        [Description("Date")]
        [DataSize(-1)]
        [Regex("^[0-9]*$")]
        Date = 3,
        [Description("Số điện thoại")]
        [DataSize(-1)]
        [Regex("^[0-9]{10,11}$")]
        PhoneNumber = 4,
        [Description("Email")]
        [DataSize(-1)]
        [Regex("^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$")]
        Email = 5,
        [Description("Boolean")]
        [DataSize(-1)]
        [Regex("^true|false|True|False|1|0$")]
        Boolean = 6,
        [Description("Tỷ lệ phần trăm")]
        [DataSize(-1)]
        [Regex("(^100([.]0{1,2})?)$|(^\\d{1,2}([.]\\d{0,2})?)$")]
        Percentage = 7,
        [Description("BigInt")]
        [DataSize(-1)]
        [Regex("^[0-9]*$")]
        BigInt = 8,
        [Description("Decimal")]
        [DataSize(-1)]
        [Regex("^[0-9]*(?:\\.[0-9]*)?$")]
        Decimal = 9,

        [Description("Tháng")]
        [DataSize(-1)]
        [Regex("")]
        Month = 10,

        [Description("Quý trong năm")]
        [DataSize(-1)]
        [Regex("")]
        QuarterOfYear = 11,

        [Description("Năm")]
        [DataSize(-1)]
        [Regex("")]
        Year = 12,

        [Description("Khoảng ngày")]
        [DataSize(-1)]
        [Regex("")]
        DateRange = 13
    }
}
