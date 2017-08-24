namespace TRuDI.Backend.HanAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using TRuDI.HanAdapter.Interface;
    using TRuDI.Backend.MessageHandlers;
    using TRuDI.Backend.Models;

    public class ApplicationState
    {
        private readonly NotificationsMessageHandler notificationsMessageHandler;

        private HanAdapterContainer activeHanAdapter;
        private CancellationTokenSource cts;

        public OperationMode OperationMode { get; set; }

        public string LastErrorMessage { get; set; }

        public string LastUrl { get; set; }

        public HanAdapterContainer ActiveHanAdapter => this.activeHanAdapter;

        public ApplicationState(NotificationsMessageHandler notificationsMessageHandler)
        {
            this.notificationsMessageHandler = notificationsMessageHandler;
            this.ConnectData = new ConnectDataViewModel();
        }

        public void LoadAdapter(string serverId)
        {
            this.activeHanAdapter = HanAdapterRepository.LoadAdapter(serverId);
        }

        public ConnectDataViewModel ConnectData { get; set; }
        public CertData ClientCert { get; set; }

        public IReadOnlyList<ContractInfo> Contracts { get; set; }

        public ConnectResult LastConnectResult { get; set; }

        public Dictionary<string, string> ManufacturerParameters { get; set; }

        public (byte[] data, string contentType) GetResourceFile(string path)
        {
            if (this.activeHanAdapter == null)
            {
                throw new InvalidOperationException($"There's no active HAN adapter to get resource files: {path}");
            }

            return this.activeHanAdapter.GetResourceFile(path);
        }

        public void ConnectAndLoadContracts()
        {
            this.NextPageAfterProgress = null;
            this.cts = new CancellationTokenSource();
            var ct = this.cts.Token;

            Task.Run(async () =>
                {
                    this.LastErrorMessage = null;

                    try
                    {
                        this.LastConnectResult = await this.activeHanAdapter.Connect(
                            this.ConnectData,
                            this.ClientCert,
                            this.ManufacturerParameters,
                            ct,
                            this.ProgressCallback);

                        this.Contracts = await this.activeHanAdapter.LoadAvailableContracts(ct, this.ProgressCallback);

                        await this.LoadNextPageAfterProgress("/Contracts");
                    }
                    catch (Exception ex)
                    {
                        this.LastErrorMessage = ex.Message;
                        await this.LoadNextPageAfterProgress("/Connect");
                    }
                });
        }


        private void ProgressCallback(ProgressInfo progressInfo)
        {
            this.notificationsMessageHandler.ProgressUpdate(progressInfo.Message, progressInfo.Progress);
        }

        public void CancelOperation()
        {
            this.cts?.Cancel();
        }

        public async Task LoadNextPageAfterProgress(string page)
        {
            this.NextPageAfterProgress = page;
            await this.notificationsMessageHandler.LoadNextPage(page);
        }

        public string NextPageAfterProgress { get; set; }
    }
}
