namespace TRuDI.Backend.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Microsoft.AspNetCore.Html;
    using System.Security.Cryptography;
    using System.Text;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.Models;
    using TRuDI.Models.BasicData;
    using TRuDI.TafAdapter.Interface;
    using TRuDI.TafAdapter.Interface.Taf2;

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

        public static HtmlString AddSpace(this string source, int length)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i]);
                if ((i + 1) % length == 0)
                {
                    sb.Append(" ");
                }
            }

            return new HtmlString(sb.ToString());
        }

        public static string OidToFriendlyName(this string oid)
        {
            var o = new Oid(oid);
            return o.FriendlyName;
        }

        public static string TafToFriendlyName(this TafId? id)
        {
            if (id == null)
            {
                return string.Empty;
            }

            return id.Value.TafToFriendlyName();
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

        public static string IsCompleted(this BillingPeriod billingPeriod)
        {
            return billingPeriod.End == null ? "nein" : "ja";
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
                    return "Warnung";

                case StatusPTB.Temp_Error_signed_invalid:
                    return "temporärer Fehler 1";

                case StatusPTB.Temp_Error_is_invalid:
                    return "temporärer Fehler 2";

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
                var serverId = new ServerId(value);
                return serverId.ToString();
            }
            catch
            {
                return value;
            }
        }

        public static string GetMedium(this ContractInfo contract)
        {
            var meter = contract.Meters?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(meter))
            {
                return "unbekannt";
            }

            var serverId = new ServerId(meter);

            switch (serverId.Medium)
            {
                case ObisMedium.Electricity:
                    return "Strom";

                case ObisMedium.HeatCostAllocator:
                    return "Heizkostenabrechnung";

                case ObisMedium.Cooling:
                    return "Kälte";

                case ObisMedium.Heat:
                    return "Wärme";

                case ObisMedium.Gas:
                    return "Gas";

                case ObisMedium.WaterCold:
                    return "Kaltwasser";

                case ObisMedium.WaterHot:
                    return "Warmwasser";

                case ObisMedium.Communication:
                case ObisMedium.Abstract:
                    return string.Empty;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
