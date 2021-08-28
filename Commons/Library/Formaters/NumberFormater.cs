using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Library.Formaters
{
    public static class NumberFormater
    {
        public static string Format(this decimal number, int decimalplace = 16)
        {
            var format = new StringBuilder();
            format.Append("#,#.");
            for (var i = 1; i < decimalplace; i++)
            {
                format.Append("#");
            }
            return number.ToString(format.ToString());
        }
    }
}
