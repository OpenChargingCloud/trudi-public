namespace TRuDI.HanAdapter.Interface
{
    public enum ErrorType
    {
        /// <summary>
        /// TCP connect failed.
        /// </summary>
        TcpConnectFailed,

        /// <summary>
        /// TLS handshake failed.
        /// </summary>
        TlsConnectFailed,

        /// <summary>
        /// User authentication failed.
        /// </summary>
        AuthenticationFailed,

        /// <summary>
        /// The SMGW has no data within the selected timerange.
        /// </summary>
        NoDataInSelectedTimeRange,

        /// <summary>
        /// There is no TAF profile for the consumer.
        /// </summary>
        NoTafProfileForUser,

        /// <summary>
        /// Received an error message from the device.
        /// </summary>
        DeviceError,

        /// <summary>
        /// Other error, not specified.
        /// </summary>
        Other = 255
    }
}