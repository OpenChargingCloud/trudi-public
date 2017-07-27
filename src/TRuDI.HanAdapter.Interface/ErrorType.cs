namespace TRuDI.HanAdapter.Interface
{
    public enum ErrorType
    {
        TcpConnectFailed,

        TlsConnectFailed,

        AuthenticationFailed,

        Other = 255
    }
}