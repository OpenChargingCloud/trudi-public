namespace TRuDI.HanAdapter.Test
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.LandisGyr;
    using TRuDI.HanAdapter.XmlValidation;
    using System.Collections.Generic;
    using System.Xml.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Starting test");

            RunTest().Wait();

            Log.Information("Finished test");
            Console.ReadKey();
        }

        static async Task RunTest()
        {
            var cts = new CancellationTokenSource();
            var adapter = new HanAdapterExample();
            //var adapter = new HanAdapterLandisGyr();

            var connectResult = await adapter.Connect(
                "eivu0012345678",
                new IPEndPoint(IPAddress.Parse("1.2.3.4"), 443),
                "Ivu",
                "123456",
                ////"ELGZ0012345678",
                ////new IPEndPoint(IPAddress.Parse("192.168.188.30"), 443),
                ////"consumer",
                ////"consumer",
                null,
                TimeSpan.FromSeconds(30),
                cts.Token,
                ProgressCallback);

            if (connectResult.error != null)
            {
                Log.Error("Connect failed: {@error}", connectResult.error);
                return;
            }

            Log.Information("Certificate: Issuer: {0}, Subject: {1}", connectResult.result.Certificate.Issuer, connectResult.result.Certificate.Subject);

            var contractsResult = await adapter.LoadAvailableContracts(cts.Token, ProgressCallback);
            if (contractsResult.error != null)
            {
                Log.Error("Failed to get the contracts: {@error}", contractsResult.error);
                return;
            }

            // Query the first billing period of the first contract.
            var ctx = new AdapterContext();
            ctx.Contract = contractsResult.contracts.First();
            ctx.Start = ctx.Contract.Begin;
            ctx.End = ctx.Contract.End ?? DateTime.Now;
            ctx.BillingPeriod = ctx.Contract.BillingPeriods.First();
            ctx.WithLogdata = true;

            var dataResult = await adapter.LoadData(ctx, cts.Token, ProgressCallback);
            
            if (dataResult.error != null)
            {
                Log.Error("Failed to get the data: {@error}", dataResult.error);
                return;
            }


            try
            {
                if (ctx.Contract.TafId == TafId.Taf7)
                {
                    var supplierResult = await adapter.LoadSupplierData(cts.Token, ProgressCallback);
                    if (supplierResult.error != null)
                    {
                        Log.Error("Failed to get the supplier data: {@error}", supplierResult.error);
                        return;
                    }

                    Ar2418Validation.ValidateSchema(dataResult.trudiXml);
                    Ar2418Validation.ValidateSchema(supplierResult.supplierXml);
                    var model = XmlModelParser.ParseHanAdapterModel(dataResult.trudiXml.Root.Descendants());
                    model = ModelValidation.ValidateHanAdapterModel(model);
                    var supplierModel = XmlModelParser.ParseSupplierModel(supplierResult.supplierXml.Root.Descendants());
                    supplierModel = ModelValidation.ValidateSupplierModel(supplierModel);
                    ContextValidation.ValidateContext(model, supplierModel, ctx);
                    
                    // Wenn es gewünscht ist die erzeugte Xml Datei abzuspeichern, muss die nächte Zeile auskommentiert werden. 
                    ////supplierResult.supplierXml.Save("supplierXml.xml");
                }
                else
                {
                    Ar2418Validation.ValidateSchema(dataResult.trudiXml);
                    var model = XmlModelParser.ParseHanAdapterModel(dataResult.trudiXml.Root.Descendants());
                    model = ModelValidation.ValidateHanAdapterModel(model);
                    ContextValidation.ValidateContext(model, ctx);  
                }
          
                // Wenn es gewünscht ist die erzeugte Xml Datei abzuspeichern, muss die nächte Zeile auskommentiert werden. 
                ////dataResult.trudiXml.Save($"trudiXml{ctx.Contract.TafId}.xml");
            }
            catch (AggregateException ae)
            {
                Log.Error(ae.Message.Split(">")[0]);
                ae.Flatten()?.InnerExceptions?.ToList().ForEach(ie => Log.Error(ie.Message));
                Log.Error("Validation failed.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private static void ProgressCallback(ProgressInfo progressInfo)
        {
            Log.Information("Progress Callback: {@progressInfo}", progressInfo);
        }
    }
}
