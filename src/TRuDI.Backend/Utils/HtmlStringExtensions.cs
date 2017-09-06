namespace TRuDI.Backend.Utils
{
    using System;
    using System.Globalization;

    using Microsoft.AspNetCore.Html;
    using System.Security.Cryptography;
    using System.Text;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public static class HtmlStringExtensions
    {
        public static HtmlString AddLineBreak(this string source, int lineLength)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i]);
                if ((i + 1) % lineLength == 0)
                {
                    sb.AppendLine("<br/>");
                }
            }

            return new HtmlString(sb.ToString());
        }

        public static string OidToFriendlyName(this string oid)
        {
            var o = new Oid(oid);
            return o.FriendlyName;
        }

        public static string TafToFriendlyName(this TafId id)
        {
            switch (id)
            {
                case TafId.Taf1:
                    return "TAF-1: Datensparsamer Tarif";

                case TafId.Taf2:
                    return "TAF-2: Zeitvariabler Tarif";

                case TafId.Taf6:
                    return "TAF-6: Ablesung von Messwerten im Bedarfsfall";

                case TafId.Taf7:
                    return "TAF-7: Zählerstandgangmessung";

                case TafId.Taf9:
                    return "TAF-9: Abruf der IST-Einspeisung";

                default:
                    return id.ToString().ToUpperInvariant();
            }
        }

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

        public static string IsCompleted(this BillingPeriod billingPeriod)
        {
            return billingPeriod.End == null ? "nein" : "ja";
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

        public static string ToStatusString(this IntervalReading reading)
        {
            if (reading == null || (reading.StatusPTB == null && reading.StatusFNN == null))
            {
                return string.Empty;
            }

            var status = reading.StatusPTB ?? reading.StatusFNN.MapToStatusPtb();

            switch (status)
            {
                case StatusPTB.No_Error:
                    return "kein Fehler";

                case StatusPTB.Warning:
                    return "Warung";

                case StatusPTB.Temp_Error_signed_invalid:
                    return "temporärer Fehler";

                case StatusPTB.Temp_Error_is_invalid:
                    return "temporärer Fehler";

                case StatusPTB.Fatal_Error:
                    return "fataler Fehler";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToStatusBackground(this IntervalReading reading)
        {
            if (reading == null)
            {
                return string.Empty;
            }

            if (reading.Value == null)
            {
                return "bg-warning";
            }

            if (reading.StatusPTB == null && reading.StatusFNN == null)
            {
                return string.Empty;
            }

            var status = reading.StatusPTB ?? reading.StatusFNN.MapToStatusPtb();

            switch (status)
            {
                case StatusPTB.No_Error:
                    return string.Empty;

                case StatusPTB.Warning:
                    return "bg-warning";

                case StatusPTB.Temp_Error_signed_invalid:
                    return "bg-warning";

                case StatusPTB.Temp_Error_is_invalid:
                    return "bg-warning";

                case StatusPTB.Fatal_Error:
                    return "bg-danger";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToStatusIcon(this IntervalReading reading)
        {
            if (reading == null || (reading.StatusPTB == null && reading.StatusFNN == null))
            {
                return string.Empty;
            }

            var status = reading.StatusPTB ?? reading.StatusFNN.MapToStatusPtb();

            switch (status)
            {
                case StatusPTB.No_Error:
                    return "fa fa-check-circle-o";

                case StatusPTB.Warning:
                    return "fa fa-check-circle";

                case StatusPTB.Temp_Error_signed_invalid:
                    return "fa fa-exclamation-circle";

                case StatusPTB.Temp_Error_is_invalid:
                    return "fa fa-exclamation-triangle";

                case StatusPTB.Fatal_Error:
                    return "fa fa-times-circle";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetOriginalValueListIdent(this OriginalValueList ovl)
        {
            return $"ovl_{ovl.Meter}_{ovl.Obis.ToHexString()}_{ovl.MeasurementPeriod.TotalSeconds}";
        }

        public static string ToServiceCategoryString(this Kind? kind)
        {
            if (kind == null)
            {
                return string.Empty;
            }

            switch (kind.Value)
            {
                case Kind.electricity:
                    return "Strom";

                case Kind.gas:
                    return "Gas";

                case Kind.water:
                    return "Wasser";

                case Kind.pressure:
                    return "Druck";

                case Kind.heat:
                    return "Wärme";

                case Kind.cold:
                    return "Kälte";

                case Kind.communication:
                    return "Kommunikation";

                case Kind.time:
                    return "Zeit";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToHistoricValueDescription(this HistoricConsumption value)
        {
            switch (value.UnitOfTime)
            {
                case TimeUnit.Day:
                    return value.Begin.ToString("dddd, dd.MM.yyyy", CultureInfo.GetCultureInfo("DE"));

                case TimeUnit.Week:
                    var cal = new GregorianCalendar();
                    var week = cal.GetWeekOfYear(value.Begin, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    return $"Woche {week}/{value.Begin.Year}, {value.Begin.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("DE"))} bis {value.End.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("DE"))}";

                case TimeUnit.Month:
                    return value.Begin.ToString("MMMM yyyy");

                case TimeUnit.Year:
                    return value.Begin.ToString("yyyy");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToFormattedDeviceId(this string value)
        {
            try
            {
                var serverId = new TRuDI.Backend.ServerId(value);
                return serverId.ToString();
            }
            catch
            {
                return value;
            }
        }
    }
}
