using System;
using Verp.Resources.Library.Formaters;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Formaters
{
    public static class DateTimeFormater
    {
        public static string Format(this DateTime date, int timezoneOffset)
        {
            var dateTimezone = date.AddMinutes(-timezoneOffset);
            return string.Format($"{{0:{DateTimeFormaterFormat.Date}}}", dateTimezone);
        }

        public static string FormatDate(this long date, int timezoneOffset)
        {
            var dateTime = date.UnixToDateTime();
            var dateTimezone = dateTime.Value.AddMinutes(-timezoneOffset);
            return string.Format($"{{0:{DateTimeFormaterFormat.Date}}}", dateTimezone);
        }
    }
}
