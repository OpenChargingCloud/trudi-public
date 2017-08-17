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
    }
}
