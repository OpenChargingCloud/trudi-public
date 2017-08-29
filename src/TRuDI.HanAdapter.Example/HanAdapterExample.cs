﻿namespace TRuDI.HanAdapter.Example
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Newtonsoft.Json;

    using TRuDI.HanAdapter.Example.Logging;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.Example.Components;
    using TRuDI.HanAdapter.XmlValidation.Models;
    using TRuDI.HanAdapter.XmlValidation.Models.BasicData;
    using TRuDI.HanAdapter.Example.ConfigModel;

    /// <inheritdoc />
    /// <summary>
    /// This isn't a real HAN adapter. It loads a configuration from a JSON file an simulates an adapter with dummy data.
    /// </summary>
    /// <seealso cref="T:TRuDI.HanAdapter.Interface.IHanAdapter" />
    public class HanAdapterExample : IHanAdapter
    {
        /// <summary>
        /// For logging is LibLog used, see https://github.com/damianh/LibLog.
        /// </summary>
        private readonly ILog logger = LogProvider.For<HanAdapterExample>();

        private HanAdapterExampleConfig config;
        private Certificate certificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HanAdapterExample"/> class.
        /// </summary>
        /// <param name="configFile">The configuration file or a directory that contains at least one configuration file.</param>
        public HanAdapterExample(string configFile)
        {
            try
            {
                var json = System.IO.File.ReadAllText(configFile);
                this.config = JsonConvert.DeserializeObject<HanAdapterExampleConfig>(json);
                this.certificate = new Certificate();
                this.certificate.HexStringToByteArray(config.Cert);
            }
            catch (Exception ex)
            {
                this.logger.ErrorException($"Failed to load test configuration from file {configFile}", ex);
                throw;
            }
        }

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
            var realEndpoint = new IPEndPoint(IPAddress.Parse(config.IPAddress), config.IPPort);
            if (!endpoint.Address.Equals(realEndpoint.Address) || endpoint.Port != realEndpoint.Port)
            {
                return (null, new AdapterError(ErrorType.TcpConnectFailed, $"{endpoint} could not be found."));
            }
            
            if(deviceId != config.DeviceId)
            {
                return(null, new AdapterError(ErrorType.TcpConnectFailed, $"{deviceId} could not be found."));
            }
            
            if (user == config.User && password == config.Password)
            {
                return await this.CommonConnect(deviceId, timeout, ct, progressCallback);
            }
            else
            {
                return (null, new AdapterError(ErrorType.AuthenticationFailed, "user or password are invalid."));
            }
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
            return await this.CommonConnect(deviceId, timeout, ct, progressCallback);
        }

        private async Task<(ConnectResult connectResult, AdapterError error)> CommonConnect(
            string deviceId, 
            TimeSpan timeout, 
            CancellationToken ct, 
            Action<ProgressInfo> progressCallback)
        {
            progressCallback(new ProgressInfo("Anmeldung am Gateway..."));

            await Task.Delay(config.TimeToConnect);

            if(config.TimeToConnect > timeout)
            {
                return (null, new AdapterError(ErrorType.Other, "Request timeout"));
            }

            progressCallback(new ProgressInfo(100, "Anmeldung am Gateway erfolgreich"));

            return (new ConnectResult(certificate.GetCert(), new FirmwareVersion[] { config.Version }), null);
        }

        public async Task<(IReadOnlyList<ContractInfo> contracts, AdapterError error)> LoadAvailableContracts(CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            var contracts = new List<ContractInfo>();
            var dataForContractExists = false;

            for(int i = 0; i < config.Contracts.Count; i++)
            {
                progressCallback(new ProgressInfo(i * (100/config.Contracts.Count), $"Vertrag {i+1} von {config.Contracts.Count}..."));

                if(config.Contracts[i].TafName == config.XmlConfig.TariffName)
                {
                    dataForContractExists = true;
                    progressCallback(new ProgressInfo((i *(100 / config.Contracts.Count)), $"Abrufbare Daten für Vertrag {i+1} vorhanden."));
                    contracts.Add(config.Contracts[i]);
                }
                await Task.Delay(1000);
            }

            if (!dataForContractExists)
            {
                return (null, new AdapterError(ErrorType.Other, "No Data found for the available contracts."));
            }

            progressCallback(new ProgressInfo(100, $"Alle Verträge geladen."));
            return (contracts, null);
        }

        public async Task<(XDocument trudiXml, AdapterError error)> LoadData(AdapterContext ctx, CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            progressCallback(new ProgressInfo(0, $"Daten werden abgerufen."));

            await Task.Delay(1000);

            if(ctx.WithLogdata != config.WithLogData)
            {
                config.WithLogData = ctx.WithLogdata;
            }

            config.BillingPeriod = ctx.BillingPeriod;

            config.Contract = ctx.Contract;

            config.Start = ctx.Start != null ? ctx.Start : ctx.BillingPeriod.Begin;

            config.End = ctx.End != null ? ctx.End : ctx.BillingPeriod.End;

            if (!config.End.HasValue)
            {
                config.End = DateTime.Now.GetDateWithoutSeconds();
            }

            var trudiXml = XmlFactory.FabricateHanAdapterContent(config);

            progressCallback(new ProgressInfo(100, $"Alle Daten erfolgreich geladen."));

            return (trudiXml, null);
        }

        public async Task<(XDocument supplierXml, AdapterError error)> LoadSupplierData(CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            progressCallback(new ProgressInfo(0, $"Die Abrechnungsdaten werden abgerufen."));

            await Task.Delay(1000);

            progressCallback(new ProgressInfo(100, $"Abrechnungsdaten vollständig geladen."));

            return (config.SupplierXml, null);
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
            await Task.Delay(500);

            return (null, null);
        }

        /// <summary>
        /// Closes the connection to the gateway.
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            await Task.Delay(1000);

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
