namespace TRuDI.Backend.Exceptions
{
    using System;

    using TRuDI.HanAdapter.Interface;

    /// <summary>
    /// Unknown TAF adapter.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class UnknownTafAdapterException : Exception
    {
        /// <summary>
        /// Gets or sets the TAF identifier without corresponding adapter.
        /// </summary>
        public TafId TafId { get; set; }
    }
}
