namespace TRuDI.HanAdapter.XmlValidation.Models
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

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
            return interval.Start.AddSeconds(Convert.ToDouble(interval.Duration));
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
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }

        /// <summary>
        /// Teilt den FNN Status hex String in die beiden Enumerationen SmgwStatusWord und BzStatusWord auf
        /// </summary>
        /// <param name="statusFNN"></param>
        public static void SplitStringToEnums(this StatusFNN statusFNN)
        {
            var smgwStat = Convert.ToInt64(statusFNN.Status.Substring(0, 8), 16);
            var bzStat = Convert.ToInt64(statusFNN.Status.Substring(8), 16);
            statusFNN.SmgwStatusWord = (SmgwStatusWord)smgwStat;
            statusFNN.BzStatusWord = (BzStatusWord)bzStat;
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
    }
}
