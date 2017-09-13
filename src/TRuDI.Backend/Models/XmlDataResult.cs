namespace TRuDI.Backend.Models
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.Models;
    using TRuDI.Models.BasicData;

    /// <summary>
    /// Holds the XML data from the SMGW/HAN adapter.
    /// </summary>
    public class XmlDataResult
    {
        /// <summary>
        /// Gets or sets the raw XML.
        /// </summary>
        public XDocument Raw { get; set; }

        /// <summary>
        /// Gets or sets the parsed model of the XML file.
        /// </summary>
        public UsagePointAdapterTRuDI Model { get; set; }

        /// <summary>
        /// Gets or sets the a shortcut to the original value lists.
        /// </summary>
        public IReadOnlyList<OriginalValueList> OriginalValueLists { get; set; }

        /// <summary>
        /// Gets or sets the meter readings.
        /// </summary>
        public IReadOnlyList<MeterReading> MeterReadings { get; set; }

        /// <summary>
        /// Gets or sets the begin of the billing period.
        /// </summary>
        public DateTime? Begin { get; set; }

        /// <summary>
        /// Gets or sets the end of the billing period.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Gets the TAF identifier from the analysis profile if it exists within the data.
        /// </summary>
        public TafId? TafId => this.Model?.AnalysisProfile?.TariffUseCase;
    }
}
