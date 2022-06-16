using System;
using Verp.Resources.Library.Formaters;

namespace VErp.Commons.Library.Formaters
{
    public static class DateTimeFormater
    {
        public static string Format(this DateTime date)
        {
            return string.Format($"{{0:{DateTimeFormaterFormat.Date}}}", date);
        }
    }
}
