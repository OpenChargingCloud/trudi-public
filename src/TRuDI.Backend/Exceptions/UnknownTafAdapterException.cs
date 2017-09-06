namespace TRuDI.Backend.Exceptions
{
    using System;

    using TRuDI.HanAdapter.Interface;

    public class UnknownTafAdapterException : Exception
    {
        public TafId TafId { get; set; }
    }
}
