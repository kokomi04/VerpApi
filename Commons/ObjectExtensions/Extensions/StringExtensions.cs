using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.ObjectExtensions.Extensions
{
    public static class StringExtensions
    {
        public static string Format(this string str,params object[] args)
        {
            return string.Format(str, args);
        }
    }
}
