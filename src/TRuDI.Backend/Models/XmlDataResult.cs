namespace TRuDI.Backend.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public class XmlDataResult
    {
        public XDocument Raw { get; set; }

        public UsagePointAdapterTRuDI Model { get; set; }

        public IReadOnlyList<OriginalValueList> OriginalValueLists { get; set; }

        public IReadOnlyList<MeterReading> MeterReadings { get; set; }

        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
    }
}
