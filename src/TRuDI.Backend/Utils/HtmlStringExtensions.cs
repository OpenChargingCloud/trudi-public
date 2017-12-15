namespace TRuDI.Backend.Utils
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Microsoft.AspNetCore.Html;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.Models;
    using TRuDI.Models.BasicData;

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

        public static string ToStatusString(this IntervalReading reading, int count = 1)
        {
            if (reading == null || (reading.StatusPTB == null && reading.StatusFNN == null))
            {
                return string.Empty;
            }

            var status = reading.StatusPTB ?? reading.StatusFNN.MapToStatusPtb();
            return status.GetStatusString(count);
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
                case StatusPTB.NoError:
                    return string.Empty;

                case StatusPTB.Warning:
                    return "bg-warning";

                case StatusPTB.TemporaryError:
                    return "bg-warning";

                case StatusPTB.CriticalTemporaryError:
                    return "bg-warning";

                case StatusPTB.FatalError:
                    return "bg-danger";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetStatusString(this StatusPTB status, int count = 0)
        {
            switch (status)
            {
                case StatusPTB.NoError:
                    return "keine Fehler";

                case StatusPTB.Warning:
                    return count == 1 ? "Warnung" : "Warnungen";

                case StatusPTB.TemporaryError:
                    return count == 1 ? "temporärer Fehler" : "temporäre Fehler";

                case StatusPTB.CriticalTemporaryError:
                    return count == 1 ? "kritischer temporärer Fehler" : "kritische temporäre Fehler";

                case StatusPTB.FatalError:
                    return count == 1 ? "fataler Fehler" : "fatale Fehler";

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
            return status.ToStatusIcon();
        }

        public static string ToStatusIcon(this StatusPTB status)
        {
            switch (status)
            {
                case StatusPTB.NoError:
                    return "fa fa-check-circle-o";

                case StatusPTB.Warning:
                    return "fa fa-check-circle";

                case StatusPTB.TemporaryError:
                    return "fa fa-exclamation-circle";

                case StatusPTB.CriticalTemporaryError:
                    return "fa fa-exclamation-triangle";

                case StatusPTB.FatalError:
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
                case Kind.Electricity:
                    return "Strom";

                case Kind.Gas:
                    return "Gas";

                case Kind.Water:
                    return "Wasser";

                case Kind.Pressure:
                    return "Druck";

                case Kind.Heat:
                    return "Wärme";

                case Kind.Cold:
                    return "Kälte";

                case Kind.Communication:
                    return "Kommunikation";

                case Kind.Time:
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

        public static string GetLogLevelString(this Level level)
        {
            switch (level)
            {
                case Level.INFO:
                    return "Info";

                case Level.WARNING:
                    return "Warnung";

                case Level.ERROR:
                    return "Fehler";

                case Level.FATAL:
                    return "Fataler Fehler";

                case Level.EXTENSION:
                    return "Erweiterung";

                default:
                    return level.ToString();
            }
        }

        public static string GetOutcomeString(this Outcome? outcome)
        {
            if (outcome == null)
            {
                return string.Empty;
            }

            switch (outcome)
            {
                case Outcome.SUCCESS:
                    return "Erfolgreich";

                case Outcome.FAILURE:
                    return "Fehlgeschlagen";

                case Outcome.EXTENSION:
                    return "Erweiterung";

                default:
                    return outcome.ToString();
            }
        }

        public static string GetMeasurementPeriodString(this TimeSpan measurementPeriod)
        {
            switch (measurementPeriod.TotalSeconds)
            {
                case 0:
                    return "unbekannt";

                case 900:
                    return "15 Minuten";

                case 1800:
                    return "30 Minuten";

                case 3600:
                    return "1 Stunde";

                case 86400:
                    return "1 Tag";

                case 86400 * 28:
                case 86400 * 29:
                case 86400 * 30:
                case 86400 * 31:
                    return "1 Monat";

                case 86400 * 365:
                case 86400 * 366:
                    return "1 Jahr";

                default:
                    if ((int)measurementPeriod.TotalSeconds % 3600 == 0)
                    {
                        return $"{measurementPeriod.TotalSeconds / 3600} Stunden";
                    }

                    if ((int)measurementPeriod.TotalSeconds % 60 == 0)
                    {
                        return $"{measurementPeriod.TotalSeconds / 60} Minuten";
                    }

                    return $"{measurementPeriod.TotalSeconds} Sekunden";
            }
        }
    }
}
