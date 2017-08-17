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
    using TRuDI.HanAdapter.XmlValidation;

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
        }

        static async Task RunTest()
        {
            var cts = new CancellationTokenSource();
            var adapter = new HanAdapterExample();

            var connectResult = await adapter.Connect(
                "Exxx0012345678",
                new IPEndPoint(IPAddress.Parse("1.2.3.4"), 443),
                "user",
                "password",
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
                Ar2418Validation.ValidateSchema(dataResult.trudiXml);
                var model = XmlModelParser.ParseHanAdapterModel(dataResult.trudiXml.Root.Descendants());
                model = ModelValidation.ValidateModel(model);
                ContextValidation.ValidateContext(model, ctx);
            }
            catch (AggregateException ae)
            {
                Log.Error(ae.Message.Split(">")[0]);
                ae.Flatten()?.InnerExceptions?.ToList().ForEach(ie => Log.Error(ie.Message));
                Log.Error("Validation failed.");
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
