namespace TRuDI.HanAdapter.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Plugin interface for a TRuDI HAN device adapter.
    /// </summary>
    public interface IHanAdapter
    {
        /// <summary>
        /// Connect to SMGW and authenticate by username and password.
        /// </summary>
        /// <param name="deviceId">"Herstellerübergreifende Identifikationsnummer" (DIN 43863-5, e.g. "EXXX0012345678") of the SMGW</param>
        /// <param name="endpoint">The IP endpoint to conntect to (IP address and port)</param>
        /// <param name="user">The KAF username of the Letztverbraucher.</param>
        /// <param name="password">The KAF password of the Letztverbraucher.</param>
        /// <param name="manufacturerSettings">Optional manufacturer specific settings (coming from UI and going to driver).</param>
        /// <param name="timeout">Connect timeout.</param>
        /// <param name="ct">Token for user initiated cancellation.</param>
        /// <param name="progressCallback">This callback must be called regularly. At the latest before the specified timeout occurs and the connection is still in progress to get established.</param>
        /// <returns>Cert is filled with the gateway's TLS certificate when the connection is established. Otherwise the error object exists.</returns>
        Task<(X509Certificate2 cert, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            string user,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback);

        /// <summary>
        /// Connect to SMGW and authenticate by client certificate.
        /// </summary>
        /// <param name="deviceId">"Herstellerübergreifende Identifikationsnummer" (DIN 43863-5, e.g. "EXXX0012345678") of the SMGW</param>
        /// <param name="endpoint">The IP endpoint to conntect to (IP address and port)</param>
        /// <param name="pkcs12Data">Client certificate with private key in PKCS#12 format</param>
        /// <param name="password">Password used to decrypt the PKCS#12 container</param>
        /// <param name="manufacturerSettings">Optional manufacturer settings.</param>
        /// <param name="timeout">Connect timeout.</param>
        /// <param name="ct">Token for user initiated cancellation.</param>
        /// <param name="progressCallback">This callback must be called regularly. At the latest before the specified timeout occurs and the connection is still in progress to get established.</param>
        /// <returns>Cert is filled with the gateway's TLS certificate when the connection is established. Otherwise the error object exists.</returns>
        Task<(X509Certificate2 cert, AdapterError error)> Connect(
            string deviceId,
            IPEndPoint endpoint,
            byte[] pkcs12Data,
            string password,
            Dictionary<string, string> manufacturerSettings,
            TimeSpan timeout,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback);

        /// <summary>
        /// Read contract information from SMGW.
        /// </summary>
        /// <param name="ct">Token for user initiated cancellation.</param>
        /// <param name="progressCallback">This callback must be called regularly.</param>
        /// <returns></returns>
        Task<(IReadOnlyList<ContractInfo> contracts, AdapterError error)> LoadAvailableContracts(
            CancellationToken ct,
            Action<ProgressInfo> progressCallback);

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="ctx">A set of parameters which specifies what exactly is to be read out.</param>
        /// <param name="ct">Token for user initiated cancellation.</param>
        /// <param name="progressCallback">This callback must be called regularly.</param>
        /// <returns></returns>
        Task<(XDocument trudiXml, AdapterError error)> LoadData(
            AdapterContext ctx, 
            CancellationToken ct,
            Action<ProgressInfo> progressCallback);

        Task Disconnect();
        
        ViewComponent SmgwImageViewComponent { get; }

        ViewComponent ManufacturerParametersViewComponent { get; }
    }
}