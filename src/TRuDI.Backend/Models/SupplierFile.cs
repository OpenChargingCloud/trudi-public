namespace TRuDI.Backend.Models
{
    using System.IO;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.TafAdapter.Interface;

    public class SupplierFile
    {
        public string Filename { get; set; }
        public string DownloadUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public MemoryStream Data { get; set; }
        public XDocument Xml { get; set; }
        public UsagePointLieferant Model { get; set; }

        public string DigestRipemd160 { get; set; }
        public string DigestSha3 { get; set; }

        public AdapterContext Ctx { get; set; }

        public IAccountingPeriod AccountingPeriod { get; set; }
    }
}