namespace TRuDI.TafAdapter.Interface
{
    using System.Diagnostics;

    using TRuDI.Models;

    [DebuggerDisplay("{ObisCode}, TariffId={TariffId}, Amount={Amount}")]
    public class Register
    {
        public ObisId ObisCode { get; set; }
        public ushort TariffId { get; set; }
    
        public long? Amount
        {
            get; set;
        }
    }
}
