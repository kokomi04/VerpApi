using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Constants
{
    public static class RegexDocExpression
    {
        public const string StartWithFuntion = "=";
        public const string DetectMainTable = "table=";

        public const string PrintTemplatePattern = @"{{([\w,$\[\].()='*@?]+)}}";
    }
}
