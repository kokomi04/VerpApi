using System;

namespace VErp.Commons.GlobalObject
{
    public static class DateTimeExtensions
    {
        public static long GetUnixUtc(this DateTime dateTime, int? timezoneOffset)
        {
            dateTime = dateTime.AddMinutes(timezoneOffset ?? 0);
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long GetUnix(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long? GetUnix(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return (long)dateTime.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime? UnixToDateTime(this long unixTime)
        {
            return UnixToDateTime((long?)unixTime, null);
        }

        public static DateTime? UnixToDateTime(this long? unixTime)
        {
            return UnixToDateTime(unixTime, null);
        }

        public static DateTime? UnixToDateTime(this long? unixTime, int? timezoneOffset)
        {
            if (!unixTime.HasValue) return null;
            return new DateTime(1970, 1, 1).AddSeconds(unixTime.Value).AddMinutes(-timezoneOffset ?? 0);
        }

        public static DateTime UnixToDateTime(this long unixTime, int? timezoneOffset)
        {
            return UnixToDateTime((long?)unixTime, timezoneOffset).Value;
        }

        public static DateTime UtcToTimeZone(this DateTime datetime, int? timezoneOffset)
        {
            return datetime.AddMinutes(-timezoneOffset ?? 0);
        }

        public static DateTime ToUtc(this DateTime datetime, int? timezoneOffset)
        {
            return datetime.AddMinutes(timezoneOffset ?? 0);
        }

        public static bool InRange(this DateTime dateTime, DateTime from, DateTime to)
        {
            return dateTime >= from && dateTime <= to;
        }

    }
}
