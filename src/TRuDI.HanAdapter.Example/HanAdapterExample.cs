namespace TRuDI.HanAdapter.Example
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using TRuDI.HanAdapter.Example.Logging;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.Example.Components;

    /// <summary>
    /// This isn't a really running example! It should serve as a starting point only.
    /// It's just simulating a communication with a SMGW and generats dummy data.
    /// </summary>
    /// <seealso cref="TRuDI.HanAdapter.Interface.IHanAdapter" />
    public class HanAdapterExample : IHanAdapter
    {
        /// <summary>
        /// For logging is LibLog used, see https://github.com/damianh/LibLog.
        /// </summary>
        private readonly ILog logger = LogProvider.For<HanAdapterExample>();

        public async Task<(ConnectResult result, AdapterError error)> Connect(
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
            return await CommonConnect(deviceId, ct, progressCallback);
        }

        public async Task<(ConnectResult result, AdapterError error)> Connect(
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
            return await CommonConnect(deviceId, ct, progressCallback);
        }

        private async Task<(ConnectResult connectResult, AdapterError error)> CommonConnect(string deviceId, CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            progressCallback(new ProgressInfo("Anmeldung am Gateway..."));

            await Task.Delay(1000);

            progressCallback(new ProgressInfo(100, "Anmeldung am Gateway erfolgreich"));

            return (
                new ConnectResult(
                    new X509Certificate2(),
                    new FirmwareVersion[]
                    {
                        new FirmwareVersion { Component = "System", Version = "1.0.0", Hash = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08" }
                    }),
                null);
        }

        public async Task<(IReadOnlyList<ContractInfo> contracts, AdapterError error)> LoadAvailableContracts(CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            var contracts = new List<ContractInfo>();

            for (int i = 0; i < 5; i++)
            {
                progressCallback(new ProgressInfo(i * 20, $"Vertrag {0} von 5..."));

                await Task.Delay(1000);

                contracts.Add(new ContractInfo()
                {
                    Begin = new DateTime(2017, 1, 1),
                    End = null,
                    ConsumerId = "consumer-01",
                    Description = $"Vertrag {i}",
                });
            }

            progressCallback(new ProgressInfo(100, $"Alle Verträge geladen."));
            return (contracts, null);
        }

        public async Task<(XDocument trudiXml, AdapterError error)> LoadData(AdapterContext ctx, CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the current register values of the specified contract.
        /// </summary>
        /// <param name="contract">The contract to .</param>
        /// <param name="ct">Token for user initiated cancellation.</param>
        /// <param name="progressCallback">This callback must be called regularly.</param>
        /// <returns>
        /// On success, a XML document containing a meter reading with the current tariff registers. 
        /// </returns>
        public async Task<(XDocument trudiXml, AdapterError error)> GetCurrentRegisterValues(
            ContractInfo contract,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
        {
            return (null, null);
        }

        /// <summary>
        /// Closes the connection to the gateway.
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
        }

        /// <summary>
        /// Gets the type of the view component that shows an image of the SMGW.
        /// </summary>
        public Type SmgwImageViewComponent => typeof(GatewayImageExampleView);

        /// <summary>
        /// Gets the manufacturer parameters view component.
        /// </summary>
        public Type ManufacturerParametersViewComponent => typeof(ManufacturerParametersExampleView);
    }
}
