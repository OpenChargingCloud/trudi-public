namespace TRuDI.Backend.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TRuDI.Backend.MessageHandlers;
    using TRuDI.Backend.Models;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.XmlValidation;
    using TRuDI.HanAdapter.XmlValidation.Models;

    public class ApplicationState
    {
        private readonly NotificationsMessageHandler notificationsMessageHandler;

        private HanAdapterContainer activeHanAdapter;
        private CancellationTokenSource cts;

        public OperationMode OperationMode { get; set; }

        public List<string> LastErrorMessages { get; } = new List<string>();

        public Stack<string> LastUrl { get; } = new Stack<string>();

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

        public ConnectResult LastConnectResult { get; private set; }

        public Dictionary<string, string> ManufacturerParameters { get; set; }

        public AdapterContext CurrentAdapterContext { get; private set; }

        public ProgressData CurrentProgressState { get; } = new ProgressData();

        public XmlDataResult CurrentDataResult { get; private set; }

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
            this.CurrentProgressState.Reset("Verbindungsaufbau", "_ConnectingPartial");
            this.cts = new CancellationTokenSource();
            var ct = this.cts.Token;

            Task.Run(async () =>
                {
                    this.LastErrorMessages.Clear();

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
                        this.LastErrorMessages.Add(ex.Message);
                        await this.LoadNextPageAfterProgress("/Connect");
                    }
                });
        }

        public void LoadData(AdapterContext ctx)
        {
            this.CurrentAdapterContext = ctx;

            this.CurrentProgressState.Reset("Daten laden", "_LoadingDataPartial");
            this.cts = new CancellationTokenSource();
            var ct = this.cts.Token;

            Task.Run(async () =>
                {
                    this.LastErrorMessages.Clear();

                    try
                    {
                        this.CurrentDataResult = await this.activeHanAdapter.LoadData(ctx, ct, this.ProgressCallback);

                        try
                        {
                            Ar2418Validation.ValidateSchema(this.CurrentDataResult.Raw);
                            this.CurrentDataResult.Model = XmlModelParser.ParseHanAdapterModel(this.CurrentDataResult?.Raw?.Root?.Descendants());
                            ModelValidation.ValidateHanAdapterModel(this.CurrentDataResult.Model);
                            ContextValidation.ValidateContext(this.CurrentDataResult.Model, ctx);
                        }
                        catch (AggregateException ex)
                        {
                            foreach (var err in ex.InnerExceptions)
                            {
                                this.LastErrorMessages.Add(err.Message);
                            }

                            await this.LoadNextPageAfterProgress("/DataView/ValidationError");
                            return;
                        }
                        catch (Exception ex)
                        {
                            this.LastErrorMessages.Add(ex.Message);

                            await this.LoadNextPageAfterProgress("/DataView/ValidationError");
                            return;
                        }

                        this.CurrentDataResult.OriginalValueLists = 
                            this.CurrentDataResult.Model.MeterReadings.Where(mr => mr.IsOriginalValueList()).Select(mr => new OriginalValueList(mr)).ToList();

                        var meterReadings = this.CurrentDataResult.Model.MeterReadings.Where(mr => !mr.IsOriginalValueList()).ToList();

                        meterReadings.Sort((a, b) => String.Compare(a.ReadingType.ObisCode, b.ReadingType.ObisCode, StringComparison.InvariantCultureIgnoreCase));
                        this.CurrentDataResult.MeterReadings = meterReadings;

                        await this.LoadNextPageAfterProgress("/DataView");
                    }
                    catch (Exception ex)
                    {
                        this.LastErrorMessages.Add(ex.Message);
                        await this.LoadNextPageAfterProgress("/Contracts");
                    }
                });
        }


        private void ProgressCallback(ProgressInfo progressInfo)
        {
            this.CurrentProgressState.StatusText = progressInfo.Message;
            this.CurrentProgressState.Progress = progressInfo.Progress;

            this.notificationsMessageHandler.ProgressUpdate(progressInfo.Message, progressInfo.Progress);
        }

        public void CancelOperation()
        {
            this.cts?.Cancel();
        }

        public async Task LoadNextPageAfterProgress(string page)
        {
            this.CurrentProgressState.NextPageAfterProgress = page;
            await this.notificationsMessageHandler.LoadNextPage(page);
        }


    }
}
