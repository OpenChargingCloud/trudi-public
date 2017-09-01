namespace TRuDI.Backend.Utils
{
    using System;

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
            if (reading.StatusPTB == null)
            {
                var x = reading.StatusFNN.SmgwStatusWord + " " + reading.StatusFNN.BzStatusWord;
                return x;
            }

            return reading.StatusPTB.ToString();
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
    }
}
