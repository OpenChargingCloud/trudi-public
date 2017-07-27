namespace TRuDI.HanAdapter.Example
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using IVU.Http;
    using IVU.Http.Http;

    using Microsoft.AspNetCore.Mvc;

    using TRuDI.HanAdapter.Example.Logging;
    using TRuDI.HanAdapter.Interface;

    /// <summary>
    /// This isn't a really running example! It should serve as a starting point only.
    /// </summary>
    /// <seealso cref="TRuDI.HanAdapter.Interface.IHanAdapter" />
    public class HanAdapterExample : IHanAdapter
    {
        /// <summary>
        /// For logging is LibLog used, see https://github.com/damianh/LibLog.
        /// </summary>
        private readonly ILog logger = LogProvider.For<HanAdapterExample>();

        /// <summary>
        /// This is the IVU.Http.HttpClient. It's a modified version with a fully managed TLS implementation.
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// The base URI: filled by Connect()
        /// </summary>
        private string baseUri;

        public async Task<(X509Certificate2 cert, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            string user,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
        {
            this.logger.Info("Connecting to {0} using user/password authentication", endpoint);

            this.baseUri = $"https://{endpoint.Address}:{endpoint.Port}/base/path/to/data";

            var clientHandler = new IVU.Http.HttpClientHandler
                                    {
                                        AutomaticDecompression = DecompressionMethods.GZip
                                    };

            X509Certificate2 serverCert = null;
            clientHandler.ServerCertificateCustomValidationCallback += (message, cert, chain, policyErrors) =>
                {
                    // Important: chain an policyErrors are currently not filled
                    serverCert = new X509Certificate2(cert);

                    // accept the server certificate and continue with TLS handshake
                    return true;
                };

            // This example gateway uses Digest Access Authentication: add the DigestAuthMessageHandler to the client handler chain:
            var digestAuthMessageHandler = new DigestAuthMessageHandler(clientHandler, user, password);

            // Create the HttpClient instance
            this.client = new IVU.Http.HttpClient(digestAuthMessageHandler);

            // Set headers common for all calls
            this.client.DefaultRequestHeaders.Add("SMGW-ID", deviceId);

            // If there's a header value that changes with every request, create a HttpRequestMessage...
            var req = new IVU.Http.HttpRequestMessage(HttpMethod.Get, this.baseUri + "/login");
            req.Headers.Add("Request-GUID", Guid.NewGuid().ToString());

            try
            {
                // ... and call client.SendAsync with it. Otherwise client.GetAsync() can also be used.
                var loginResult = await this.client.SendAsync(req, ct);
                if (!loginResult.IsSuccessStatusCode)
                {
                    return (null, new AdapterError(ErrorType.AuthenticationFailed));
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorException("Connect failed to {0}", ex, endpoint);
                return (null, new AdapterError(ErrorType.AuthenticationFailed));
            }

            return (serverCert, null);
        }

        public async Task<(X509Certificate2 cert, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            byte[] pkcs12Data,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
        {
            this.logger.Info("Connecting to {0} using a client certificate", endpoint);

            this.baseUri = $"https://{endpoint.Address}:{endpoint.Port}/base/path/to/data";

            var clientHandler = new IVU.Http.HttpClientHandler
                                    {
                                        AutomaticDecompression = DecompressionMethods.GZip 
                                    };
            
            // Load the client certificate
            clientHandler.ClientCertificate = new ClientCertificateWithKey(pkcs12Data, password);

            X509Certificate2 serverCert = null;
            clientHandler.ServerCertificateCustomValidationCallback += (message, cert, chain, policyErrors) =>
                {
                    // Important: chain an policyErrors are currently not filled
                    serverCert = new X509Certificate2(cert);

                    // accept the server certificate and continue with TLS handshake
                    return true;
                };

            // Create the HttpClient instance
            this.client = new IVU.Http.HttpClient(clientHandler);

            // Set headers common for all calls
            this.client.DefaultRequestHeaders.Add("SMGW-ID", deviceId);

            // If there's a header value that changes with every request, create a HttpRequestMessage...
            var req = new IVU.Http.HttpRequestMessage(HttpMethod.Get, this.baseUri + "/login");
            req.Headers.Add("Request-GUID", Guid.NewGuid().ToString());

            try
            {
                // ... and call client.SendAsync with it. Otherwise client.GetAsync() can also be used.
                var loginResult = await this.client.SendAsync(req, ct);
                if (!loginResult.IsSuccessStatusCode)
                {
                    return (null, new AdapterError(ErrorType.AuthenticationFailed));
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorException("Connect failed to {0}", ex, endpoint);
                return (null, new AdapterError(ErrorType.AuthenticationFailed));
            }

            return (serverCert, null);
        }

        public Task<(IReadOnlyList<ContractInfo> contracts, AdapterError error)> LoadAvailableContracts(CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            throw new NotImplementedException();
        }

        public Task<(XDocument trudiXml, AdapterError error)> LoadData(AdapterContext ctx, CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public ViewComponent SmgwImageViewComponent { get; }

        public ViewComponent ManufacturerParametersViewComponent { get; }
    }
}
