using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Library.Resources.Formaters;

namespace VErp.Commons.Library.Formaters
{
    public static class DateTimeFormater
    {
        public static string Format(this DateTime date)
        {
            return string.Format(DateTimeFormaterFormat.Date, date);
        }
    }
}
