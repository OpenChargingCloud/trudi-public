namespace TRuDI.HanAdapter.Test
{
    using System;
    using System.Net;
    using System.Threading;

    using TRuDI.HanAdapter.Interface;

    public class CommandParameters
    {
        public string ServerId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] ClientCert { get; set; }
        public IPEndPoint IpEndpoint { get; set; }
        public uint Timeout { get; set; }
        public IHanAdapter HanAdapter { get; set; }


        public CancellationToken CreateCancellationToken()
        {
            if (this.Timeout == 0)
            {
                return CancellationToken.None;
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(this.Timeout));
            return cts.Token;
        }
    }
}
