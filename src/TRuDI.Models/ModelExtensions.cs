﻿namespace TRuDI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    using TRuDI.Models.BasicData;
    using TRuDI.Models.CheckData;

    public static class ModelExtensions
    {
        /// <summary>
        /// Diese Funktion wird benutzt um den von der Xml Datei gelieferten Hex String in ein Byte Array umzuwandeln.
        /// </summary>
        /// <param name="cert">Die Instanz der Klasse Certificate, der das Zertifikat zugewiesen wird</param>
        /// <param name="hex">Der Hex String, der in ein Byte Array umgewandelt wird</param>
        public static void HexStringToByteArray(this Certificate cert, string hex)
        {
            cert.CertContent = Enumerable.Range(0, hex.Length)
                                .Where(x => x % 2 == 0)
                                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                .ToArray();
        }

        /// <summary>
        /// In der Funktion wird aus dem Byterarray CertContent das Zertifikat erzeugt
        /// </summary>
        /// <returns>Gibt ein X509Certificate2 zurück</returns>
        public static X509Certificate2 GetCert(this Certificate cert)
        {
            // TODO: Absichern der Funktion
            return new X509Certificate2(cert.CertContent);
        }

        /// <summary>
        /// Funktion zur Berechung des Endzeitpunkts des Intervals
        /// </summary>
        /// <returns>Den Endzeitpunkt des Intervals</returns>
        public static DateTime GetEnd(this Interval interval)
        {
            if (interval.Start.Kind != DateTimeKind.Utc)
            {
                var startTimeUtc = interval.Start.ToUniversalTime();
                return (startTimeUtc.AddSeconds(Convert.ToDouble(interval.Duration))).ToLocalTime();
            }
            else
            {
                return interval.Start.AddSeconds(Convert.ToDouble(interval.Duration));
            }
        }

        /// <summary>
        /// Funktion zur Berechung des Endzeitpunkts des Intervals
        /// </summary>
        /// <returns>Den Endzeitpunkt des Intervals</returns>
        public static DateTime GetCaptureTimeEnd(this Interval interval)
        {
            return interval.CaptureTime.AddSeconds(Convert.ToDouble(interval.Duration));
        }

        /// <summary>
        /// Funktion zum Test ob ein String einen gültigen Hex String darstellt
        /// </summary>
        /// <param name="hex">Der zu überprüfende String</param>
        /// <returns>Gibt einen Wahrheitswert zurück</returns>
        public static bool ValidateHexString(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            if (hex.Length % 2 == 1)
            {
                return false;
            }

            foreach (char c in hex)
            {
                switch (c)
                {
                    case '0':
                        break;
                    case '1':
                        break;
                    case '2':
                        break;
                    case '3':
                        break;
                    case '4':
                        break;
                    case '5':
                        break;
                    case '6':
                        break;
                    case '7':
                        break;
                    case '8':
                        break;
                    case '9':
                        break;
                    case 'a':
                        break;
                    case 'A':
                        break;
                    case 'b':
                        break;
                    case 'B':
                        break;
                    case 'c':
                        break;
                    case 'C':
                        break;
                    case 'd':
                        break;
                    case 'D':
                        break;
                    case 'e':
                        break;
                    case 'E':
                        break;
                    case 'f':
                        break;
                    case 'F':
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Die Funktion kürzt mögliche Sekundenwerte eines DateTime Objekts
        /// </summary>
        /// <param name="dateTime">Das zu kürzende DateTime Objekt</param>
        /// <returns>Das gekürzute DateTime Objekt</returns>
        public static DateTime GetDateWithoutSeconds(this DateTime dateTime)
        {
            var utcConvertedTime = dateTime.ToUniversalTime();
            var utcWithoutSeconds = new DateTime(utcConvertedTime.Year, utcConvertedTime.Month, utcConvertedTime.Day, utcConvertedTime.Hour, utcConvertedTime.Minute, 0, DateTimeKind.Utc);
            return dateTime.Kind == DateTimeKind.Utc ? utcWithoutSeconds : utcWithoutSeconds.ToLocalTime();
        }

        /// <summary>
        /// Die Funktion liefert den "geglätteten" captureTime Zeitstempel zurück
        /// </summary>
        /// <param name="captureTime">Der zu glättende Zeitwert</param>
        /// <param name="interval">The period interval.</param>
        /// <returns>Der gerundete Zeitstempel</returns>
        public static DateTime GetSmoothCaptureTime(DateTime captureTime, int interval = 900)
        {
            if (interval < 86400)
            {
                var isUtc = captureTime.Kind == DateTimeKind.Utc;
                var captureTimeUtc = captureTime.ToUniversalTime();

                var diffSpan = (int)(captureTimeUtc - captureTimeUtc.Date).TotalSeconds;
                var diff = interval - (diffSpan % interval);
                if (diff == 0 || diff == interval)
                {
                    return isUtc ? captureTimeUtc : captureTimeUtc.ToLocalTime();
                }

                var a = captureTimeUtc.Date.AddSeconds((diffSpan / interval) * interval);
                var window = interval == 900 ? 450 : interval / 100;
                captureTimeUtc = (captureTimeUtc - a).TotalSeconds <= window ? a : a.AddSeconds(interval);
                return isUtc ? captureTimeUtc : captureTimeUtc.ToLocalTime();
            }

            if (interval == 86400)
            {
                if (captureTime.Hour == 23 && captureTime.Minute >= 45)
                {
                    return captureTime.Date.AddDays(1);
                }

                if (captureTime.Hour == 0 && captureTime.Minute < 15)
                {
                    return captureTime.Date;
                }
            }

            return captureTime;
        }

        /// <summary>
        /// Returns true if the specified timestamp is valid for the specified interval.
        /// </summary>
        /// <param name="timestamp">The timestmap to check.</param>
        /// <param name="interval">The period interval.</param>
        /// <returns><c>true</c> if the timestamp is valid.</returns>
        public static bool IsValidMeasurementPeriodTimestamp(this DateTime timestamp, int interval = 900)
        {
            var diffSpan = (int)(timestamp - timestamp.Date).TotalSeconds;
            var diff = interval - (diffSpan % interval);
            if (diff == 0 || diff == interval)
            {
                return true;
            }

            var window = interval == 900 ? 240 : interval / 100;
            
            return diff <= window || diff >= (interval - window);
        }

        /// <summary>
        /// Removes readings not valid for the specified interval. Only the first interval reading isn't checked.
        /// </summary>
        /// <param name="readings">List of readings.</param>
        /// <param name="interval">The measurement period.</param>
        public static void FilterIntervalReadings(this List<IntervalReading> readings, int interval)
        {
            for (int i = 1; i < readings.Count; i++)
            {
                var reading = readings[i];
                if (!reading.TimePeriod.Start.IsValidMeasurementPeriodTimestamp(interval))
                {
                    readings.RemoveAt(i);
                    i--;
                }
            }
        }


        /// <summary>
        /// Validiert den FNNStatus 
        /// </summary>
        /// <param name="statusFNN"></param>
        /// <returns>True wenn der FNN Status gültig ist, False falls nicht</returns>
        public static bool ValidateFNNStatus(this StatusFNN statusFNN)
        {
            var binaryString = Convert.ToString(Convert.ToInt64(statusFNN.Status, 16), 2).PadLeft(64, '0');
            var mask = StatusFNN.SMGWMASK + StatusFNN.BZMASK;

            for (int index = 0; index < 64; index++)
            {
                if (mask[index] == 'x')
                {
                    continue;
                }
                else if (binaryString[index] == mask[index])
                {
                    continue;
                }
                else
                {
                    return false;
                }

            }

            return true;
        }

        public static DateTime GetDateTimeFromSpecialDayProfile(SpecialDayProfile sdp, DayTimeProfile dtp)
        {
            return new DateTime((int)sdp.SpecialDayDate.Year,
                                (int)sdp.SpecialDayDate.Month,
                                (int)sdp.SpecialDayDate.DayOfMonth,
                                (int)dtp.StartTime.Hour,
                                (int)dtp.StartTime.Minute,
                                (int)dtp.StartTime.Second);
        }

        public static DateTime GetDate(this DayVarType date)
        {
            return new DateTime((int)date.Year, (int)date.Month, (int)date.DayOfMonth);
        }

        public static bool IsDateInIntervalBlock(this Interval interval, DateTime date)
        {
            if (date >= interval.Start && date <= interval.GetEnd())
            {
                return true;
            }

            return false;
        }

        public static bool IsPeriodInIntervalBlock(this Interval interval, DateTime start, DateTime end)
        {
            if (start >= interval.Start && end <= interval.GetEnd())
            {
                return true;
            }

            return false;
        }
    }
}