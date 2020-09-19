using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Constants
{
    public static class RegexDocExpression
    {
        public const string Info = @"#{([\w|,|$|\[\]|\{|\}]+)}";
        public const string Table = @"#table{([\w|,|$|\[\]|\{|\}]+)}";
    }
}
