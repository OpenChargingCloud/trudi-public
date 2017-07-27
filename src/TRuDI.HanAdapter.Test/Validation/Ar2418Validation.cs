namespace TRuDI.HanAdapter.Test.Validation
{
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using Serilog;

    public static class Ar2418Validation
    {
        public static bool Validate(XDocument document)
        {
            var schemas = new XmlSchemaSet();
            schemas.XmlResolver = new XmlResolver();
            schemas.Add("http://vde.de/AR_2418-6.xsd", XmlReader.Create("./Validation/Schemata/AR_2418-6.xsd", new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse }));

            bool isValid = true;

            document.Validate(
                schemas, 
                (sender, eventArgs) =>
                {
                    isValid = false;
                    Log.Error("Validation failed: {0}", eventArgs.Message);
                });

            return isValid;
        }
    }
}
