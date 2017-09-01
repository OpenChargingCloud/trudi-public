﻿namespace TRuDI.Backend.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

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

        public BreadCrumbTrail BreadCrumbTrail { get; } = new BreadCrumbTrail();

        public List<string> LastErrorMessages { get; } = new List<string>();

        public HanAdapterContainer ActiveHanAdapter => this.activeHanAdapter;

        public ApplicationState(NotificationsMessageHandler notificationsMessageHandler)
        {
            this.notificationsMessageHandler = notificationsMessageHandler;
            this.ConnectData = new ConnectDataViewModel();
            this.BreadCrumbTrail.Add("Start", "/OperatingModeSelection");
        }

        public void LoadAdapter(string serverId)
        {
            this.activeHanAdapter = HanAdapterRepository.LoadAdapter(serverId);
        }

        public ConnectDataViewModel ConnectData { get; set; }
        public CertData ClientCert { get; set; }

        public IReadOnlyList<ContractContainer> Contracts { get; set; }

        public ConnectResult LastConnectResult { get; private set; }

        public Dictionary<string, string> ManufacturerParameters { get; set; }

        public AdapterContext CurrentAdapterContext { get; private set; }

        public ProgressData CurrentProgressState { get; } = new ProgressData();

        public XmlDataResult CurrentDataResult { get; private set; }

        public SupplierFile CurrentSupplierFile { get; set; }

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

                        var contracts = await this.activeHanAdapter.LoadAvailableContracts(ct, this.ProgressCallback);
                        var containers = contracts.Where(c => c.TafId != TafId.Taf6).Select(c => new ContractContainer() { Contract = c }).ToList();

                        foreach (var taf6Contract in contracts.Where(c => c.TafId == TafId.Taf6))
                        {
                            var cnt = containers.FirstOrDefault(c =>
                                taf6Contract.TafName == c.Contract.TafName &&
                                taf6Contract.Begin == c.Contract.Begin &&
                                taf6Contract.End == c.Contract.End &&
                                taf6Contract.SupplierId == c.Contract.SupplierId &&
                                taf6Contract.Meters.SequenceEqual(c.Contract.Meters));

                            if (cnt != null)
                            {
                                cnt.Taf6 = taf6Contract;
                            }
                            else
                            {
                                containers.Add(new ContractContainer { Contract = taf6Contract });
                            }
                        }

                        this.Contracts = containers;

                        if (this.OperationMode == OperationMode.DisplayFunction)
                        {
                            await this.LoadNextPageAfterProgress("/Contracts");
                        }
                        else
                        {
                            var tariffContract = this.Contracts.Select(c => c.Contract).FirstOrDefault(c => c.TafId == TafId.Taf7 && c.MeteringPointId == this.CurrentSupplierFile.Model.UsagePointId && c.TafName == this.CurrentSupplierFile.Model.AnalysisProfile.TariffId);



                        }
                    }
                    catch (Exception ex)
                    {
                        this.LastErrorMessages.Add(ex.Message);
                        await this.LoadNextPageAfterProgress("/Connect");
                    }
                });
        }

        public void LoadData(XDocument raw)
        {
            this.LastErrorMessages.Clear();
            this.LoadDataFromXml(raw, null);
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
                        var raw = await this.activeHanAdapter.LoadData(ctx, ct, this.ProgressCallback);

                        try
                        {
                            this.LoadDataFromXml(raw, ctx);
                        }
                        catch
                        {
                            // Errors are collected by LoadDataFromXml, just go to the error page now
                            await this.LoadNextPageAfterProgress("/DataView/ValidationError");
                            return;
                        }

                        await this.LoadNextPageAfterProgress("/DataView");
                    }
                    catch (Exception ex)
                    {
                        this.LastErrorMessages.Add(ex.Message);
                        await this.LoadNextPageAfterProgress("/Contracts");
                    }
                });
        }

        public void LoadSupplierXml()
        {
            this.LastErrorMessages.Clear();

            try
            {
                this.CurrentSupplierFile.Model = XmlModelParser.ParseSupplierModel(this.CurrentSupplierFile.Xml.Root.Descendants());
                ModelValidation.ValidateSupplierModel(this.CurrentSupplierFile.Model);
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    this.LastErrorMessages.Add(err.Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                this.LastErrorMessages.Add(ex.Message);
                throw;
            }
        }


        private void LoadDataFromXml(XDocument raw, AdapterContext ctx)
        {
            this.LastErrorMessages.Clear();

            try
            {
                this.CurrentDataResult = new XmlDataResult { Raw = raw };

                Ar2418Validation.ValidateSchema(raw);
                this.CurrentDataResult.Model = XmlModelParser.ParseHanAdapterModel(this.CurrentDataResult?.Raw?.Root?.Descendants());
                ModelValidation.ValidateHanAdapterModel(this.CurrentDataResult.Model);

                if (ctx != null)
                {
                    ContextValidation.ValidateContext(this.CurrentDataResult.Model, ctx);
                }
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    this.LastErrorMessages.Add(err.Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                this.LastErrorMessages.Add(ex.Message);
                throw;
            }

            this.CurrentDataResult.OriginalValueLists =
                this.CurrentDataResult.Model.MeterReadings.Where(mr => mr.IsOriginalValueList()).Select(mr => new OriginalValueList(mr)).ToList();

            var meterReadings = this.CurrentDataResult.Model.MeterReadings.Where(mr => !mr.IsOriginalValueList()).ToList();

            meterReadings.Sort((a, b) => string.Compare(a.ReadingType.ObisCode, b.ReadingType.ObisCode, StringComparison.InvariantCultureIgnoreCase));
            this.CurrentDataResult.MeterReadings = meterReadings;
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