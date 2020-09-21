using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Constants
{
    public static class RegexDocExpression
    {
        public const string Pattern = @"{{([\w,$\[\]\.\(\)='*@]+)}}";
    }
}
