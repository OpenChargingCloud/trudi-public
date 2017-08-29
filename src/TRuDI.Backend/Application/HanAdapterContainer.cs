namespace TRuDI.Backend.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using TRuDI.Backend.Exceptions;
    using TRuDI.Backend.Models;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation;

    public class HanAdapterContainer
    {
        public IHanAdapter Adapter { get; }

        private HanAdapterInfo hanAdapterInfo;

        public string DeviceId { get; }

        public string GatewayImageView => this.Adapter?.SmgwImageViewComponent?.Name;
        public string ManufacturerParametersView => this.Adapter?.ManufacturerParametersViewComponent?.Name;

        public HanAdapterContainer(HanAdapterInfo hanAdapterInfo, string deviceId)
        {
            this.DeviceId = deviceId;
            this.hanAdapterInfo = hanAdapterInfo;
            this.Adapter = hanAdapterInfo.CreateInstance();
        }
        
        public (byte[] data, string contentType) GetResourceFile(string path)
        {
            var resourceName = this.hanAdapterInfo.BaseNamespace + "." + path.Replace('/', '.');
            var stream = this.hanAdapterInfo.Assembly.GetManifestResourceStream(resourceName);
            if(stream == null)
            {
                throw new FileNotFoundException("Resource file wasn't found", resourceName);
            }

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            string contentType = string.Empty;
            if(path.EndsWith(".png"))
            {
                contentType = "image/png";
            }
            else if(path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
            {
                contentType = "image/jpeg";
            }
            
            return (data, contentType);
        }

        public async Task<ConnectResult> Connect(
            ConnectDataViewModel connectData,
            CertData clientCert,
            Dictionary<string, string> manufacturerParameters,
            CancellationToken ct,
            Action<ProgressInfo> progressCallback)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(connectData.Address), connectData.Port);
            (ConnectResult result, AdapterError error) connectResult;

            switch (connectData.AuthMode)
            {
                case AuthMode.UserPassword:
                    connectResult = await this.Adapter.Connect(
                                        connectData.DeviceId,
                                        endpoint,
                                        connectData.Username,
                                        connectData.Password,
                                        manufacturerParameters,
                                        TimeSpan.FromSeconds(30),
                                        ct,
                                        progressCallback);
                    break;

                case AuthMode.ClientCertificate:
                    connectResult = await this.Adapter.Connect(
                                        connectData.DeviceId,
                                        endpoint,
                                        clientCert.Data,
                                        clientCert.Password,
                                        manufacturerParameters,
                                        TimeSpan.FromSeconds(30),
                                        ct,
                                        progressCallback);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(connectData.AuthMode));
            }

            if (connectResult.error != null)
            {
                throw new HanAdapterException(connectResult.error);
            }

            return connectResult.result;
        }

        public async Task<XmlDataResult> LoadData(AdapterContext ctx, CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            var result = await this.Adapter.LoadData(ctx, ct, progressCallback);

            if (result.error != null)
            {
                throw new HanAdapterException(result.error);
            }

            var xmlDataResult = new XmlDataResult();
            xmlDataResult.Raw = result.trudiXml;

            return xmlDataResult;
        }

        public async Task<IReadOnlyList<ContractInfo>> LoadAvailableContracts(CancellationToken ct, Action<ProgressInfo> progressCallback)
        {
            var result = await this.Adapter.LoadAvailableContracts(ct, progressCallback);

            if (result.error != null)
            {
                throw new HanAdapterException(result.error);
            }

            return result.contracts;
        }
    }
}
