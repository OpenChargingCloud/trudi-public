namespace TRuDI.Backend.Models
{
    using System.Xml.Linq;

    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;

    public class XmlDataResult
    {
        public XDocument Raw { get; set; }

        public UsagePointAdapterTRuDI Model { get; set; }
    }
}
