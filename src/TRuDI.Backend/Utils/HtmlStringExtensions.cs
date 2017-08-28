namespace TRuDI.Backend.Utils
{
    using System;

    using Microsoft.AspNetCore.Html;
    using System.Security.Cryptography;
    using System.Text;

    using TRuDI.HanAdapter.Interface;

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
            return timestamp.ToString("dd.MM.yyyy hh:mm");
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

        public static string ToIso8601(this DateTime timestamp)
        {
            return timestamp.ToString("s");
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
