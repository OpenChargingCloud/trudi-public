namespace TRuDI.HanAdapter.Test.Validation
{
    using System;
    using System.IO;
    using System.Xml;

    using Serilog;

    public class XmlResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            var xsd = Path.GetFileName(absoluteUri.LocalPath);

            switch (xsd)
            {
                case "xml.xsd":
                    return new MemoryStream(File.ReadAllBytes("./Validation/Schemata/xml.xsd"));

                case "atom.xsd":
                    return new MemoryStream(File.ReadAllBytes("./Validation/Schemata/atom.xsd"));

                case "espi_derived.xsd":
                    return new MemoryStream(File.ReadAllBytes("./Validation/Schemata/espi_derived.xsd"));

                default:
                    if (absoluteUri.AbsolutePath.Contains("AR_2418-6_V0.96"))
                    {
                        return new MemoryStream(File.ReadAllBytes("./Validation/Schemata/AR_2418-6.xsd"));
                    }

                    Log.Error("XSD file not found: {0}" + absoluteUri);
                    break;
            }

            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }
    }
}
