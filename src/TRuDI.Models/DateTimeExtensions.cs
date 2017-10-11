namespace TRuDI.Models
{
    using System;

    public static class DateTimeExtensions
    {
        public static string ToFormatedString(this DateTime timestamp)
        {
            return timestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        }

        public static string ToFormatedString(this DateTime? timestamp)
        {
            if (timestamp == null)
            {
                return string.Empty;
            }

            return timestamp.Value.ToFormatedString();
        }

        public static DateTime GetEndTimeOrNow(this DateTime? timestamp)
        {
            if (timestamp == null)
            {
                return DateTime.UtcNow;
            }

            if (timestamp.Value.ToUniversalTime() > DateTime.UtcNow)
            {
                return DateTime.UtcNow;
            }

            return timestamp.Value;
        }

        public static DateTime RoundDown(this DateTime value, int minutes)
        {
            var diff = value.Minute % minutes;
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute - diff, 0, value.Kind);
        }

        public static string ToIso8601(this DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string ToIso8601(this DateTime? timestamp)
        {
            if (timestamp == null)
            {
                return string.Empty;
            }

            return timestamp.Value.ToIso8601();
        }
    }
}
