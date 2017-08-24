namespace TRuDI.Backend.Exceptions
{
    using System;

    using TRuDI.HanAdapter.Interface;

    public class HanAdapterException : Exception
    {
        public HanAdapterException(AdapterError error)
        {
            this.AdapterError = error;
        }

        public AdapterError AdapterError { get; }

        public override string Message => $"{this.AdapterError.Type} - {this.AdapterError.Message}";
    }
}
