namespace TRuDI.Backend.Exceptions
{
    using System;

    public class UnknownManufacturerException : Exception
    {
        public string FlagId { get; set; }
    }
}
