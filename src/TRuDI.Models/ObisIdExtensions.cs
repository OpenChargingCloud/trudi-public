namespace TRuDI.Models
{
    public static class ObisIdExtensions
    {
        public static string ToFormattedObis(this string hexId)
        {
            return new ObisId(hexId).ToString();
        }

        public static string GetMediumLabel(this ObisId id)
        {
            switch (id.Medium)
            {
                case ObisMedium.Abstract:
                    return string.Empty;

                case ObisMedium.Electricity:
                    return "Elektrizität";

                case ObisMedium.HeatCostAllocator:
                    return "Heizkostenverteiler";

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
                    return "Kommunikationsgerät";

                default:
                    return "unbekannt";
            }
        }

        public static string GetLabel(this ObisId id)
        {
            switch (id.Medium)
            {
                case ObisMedium.Electricity:
                    return GetElectricityLabel(id);

                case ObisMedium.Gas:
                    return GetGasLabel(id);

                default:
                    return $"Für die Kennziffer {id} ist keine Beschreibung hinterlegt.";
            }
        }

        private static string GetGasLabel(ObisId id)
        {
            // 7-0:3.1.0*255
            if (id.C == 3 && id.D == 1 && id.E == 0)
            {
                return "Volumen";
            }

            return $"Für die Kennziffer {id} ist keine Beschreibung hinterlegt.";
        }

        private static string GetElectricityLabel(ObisId id)
        {
            string tariff;
            switch (id.E)
            {
                case 0:
                    tariff = "Gesamt";
                    break;

                case 63:
                    tariff = "Fehlerregister";
                    break;

                default:
                    tariff = $"Tarifregister {id.E}";
                    break;
            }

            switch (id.C)
            {
                case 1:
                    return "Elektrische Wirkleistung Bezug " + tariff;

                case 2:
                    return "Elektrische Wirkleistung Lieferung " + tariff;
            }

            return $"Für die Kennziffer {id} ist keine Beschreibung hinterlegt.";
        }
    }
}