using System;
using System.Collections.Generic;
using System.Text;
using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

namespace TRuDI.HanAdapter.Example.ConfigModel
{
    public class XmlConfig
    {

        public string UsagePointId
        {
            get; set;
        }

        public string TariffName
        {
            get; set;
        }

        public string TariffId
        {
            get; set;
        }

        public string CustomerId
        {
            get; set;
        }

        public string InvoicingPartyId
        {
            get; set;
        }

        public string SmgwId
        {
            get; set;
        }

        public ushort ServiceCategoryKind
        {
            get; set;
        }

        public List<CertificateContainer> Certificates
        {
            get; set;
        }

        public bool RandomLogCount
        {
            get; set;
        }

        public int? LogCount
        {
            get; set;
        }

        public List<string> PossibleLogMessages
        {
            get; set;
        }

        public int TariffUseCase
        {
            get; set;
        }

        public int TariffStageCount
        {
            get; set;
        }

        public int DefaultTariffNumber
        {
            get; set;
        }

        public int ValueSummary
        {
            get; set;
        }

        public int[] Taf1Reg
        {
            get; set;
        }

        public byte DayIdCount
        {
            get; set;
        }

        public List<MeterReadingConfig> MeterReadingConfigs
        {
            get; set;
        }

        public List<TariffStageConfig> TariffStageConfigs
        {
            get; set;
        }

        public List<DayProfileConfig> DayProfiles
        {
            get; set;
        }

    }

    public class CertificateContainer
    {
        public Certificate Certificate
        {
            get; set;
        }

        public string CertContent
        {
            get; set;
        }
    }

    public class MeterReadingConfig
    {

        public string MeterId
        {
            get; set;
        }

        public string MeterReadingId
        {
            get; set;
        }

        public bool IsOML
        {
            get; set;
        }

        public int OMLInitValue
        {
            get; set;
        }

        public uint? PeriodSeconds
        {
            get; set;
        }

        public short? PowerOfTenMultiplier
        {
            get; set;
        }

        public ushort? Uom
        {
            get; set;
        }

        public short? Scaler
        {
            get; set;
        }

        public string ObisCode
        {
            get; set;
        }

        public List<IntervalBlockConfig> IntervalBlocks
        {
            get; set;
        }
    }

    public class IntervalBlockConfig
    {
        public uint? Duration
        {
            get; set;
        }

        public DateTime Start
        {
            get; set;
        }

        public string UsedStatus
        {
            get; set;
        }
    }

    public class TariffStageConfig
    {
        public int? TariffNumber
        {
            get; set;
        }

        public string Description
        {
            get; set;
        } 

        public string ObisCode
        {
            get; set;
        }
    }

    public class DayProfileConfig
    {
        public ushort? DayId
        {
            get; set;
        }

        public List<DayTimeProfileConfig> DayTimeProfiles
        {
            get; set;
        }
    }

    public class DayTimeProfileConfig
    {
        public DateTime Start
        {
            get; set;
        }

        public DateTime End
        {
            get; set;
        }

        public ushort? TariffNumber
        {
            get; set;
        }
    }
}
