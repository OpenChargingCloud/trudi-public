namespace TRuDI.TafAdapter.Interface
{
    using TRuDI.HanAdapter.XmlValidation.Models;

    public class Register
    {
        public ObisId ObisCode
        {
            get; set;
        }

        public ushort TariffId
        {
            get; set;
        }

        public long? Amount
        {
            get; set;
        }
    }
}
