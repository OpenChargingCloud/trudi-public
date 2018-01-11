namespace TRuDI.HanAdapter.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Serialization;

    using Microsoft.Extensions.CommandLineUtils;

    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    using TRuDI.HanAdapter.Example;
    using TRuDI.HanAdapter.Interface;
    using TRuDI.HanAdapter.Repository;
    using TRuDI.HanAdapter.Test.Models;
    using TRuDI.Models;

    class Program
    {
        static CommandOption username;
        static CommandOption password;
        static CommandOption pkcs12file;

        static CommandOption serverId;
        static CommandOption address;
        static CommandOption port;

        static CommandOption timeout;

        static CommandOption exportFile;

        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
            commandLineApplication.Description = "Mit diesem Programm können HAN-Adapter ohne TRuDI-UI getestet werden.";
            var logFileOption = commandLineApplication.Option("-l|--log <logfile>", "Datei in der Programmmeldungen protokolliert werden.", CommandOptionType.SingleValue);
            var logLevelOption = commandLineApplication.Option("--loglevel <loglevel>", "Log Level: verbose, debug, info, warning, error, fatal. Standard ist info.", CommandOptionType.SingleValue);
            var testFileOption = commandLineApplication.Option("-t|--test <testconfig>", "Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.", CommandOptionType.SingleValue);

            exportFile = commandLineApplication.Option("--output <output-file>", "Export-Datei (XML).", CommandOptionType.SingleValue);
            username = commandLineApplication.Option("--user <username>", "Benutzername", CommandOptionType.SingleValue);
            pkcs12file = commandLineApplication.Option("--cert <cert-file>", "PKCS#12-Datei mit Client-Zertifikat und dazugehörigen Key", CommandOptionType.SingleValue);
            password = commandLineApplication.Option("--pass <password>", "Passwort zum Benutzernamen oder ggf. für die PKCS#12-Datei.", CommandOptionType.SingleValue);

            serverId = commandLineApplication.Option("--id <serverid>", "Herstellerübergreifende ID des SMGW (z.B. \"EABC0012345678\")", CommandOptionType.SingleValue);
            address = commandLineApplication.Option("--addr <address>", "IP-Adresse des SMGW.", CommandOptionType.SingleValue);
            port = commandLineApplication.Option("--port <port>", "Port des SMGW.", CommandOptionType.SingleValue);

            timeout = commandLineApplication.Option("--timeout <timeout>", "Timeout in Sekunden nachdem der Vorgang über das CancellationToken abgebrochen wird.", CommandOptionType.SingleValue);
            
            commandLineApplication.Command("adapters",
                (target) =>
                    {
                        target.Description = "Liste der bekannten HAN-Adapter.";
                        target.HelpOption("-? | -h | --help");
                    }).OnExecute(() =>
                {
                    InitLogging();
                    return AdaptersCommand();
                });

            commandLineApplication.Command("contracts",
                (target) =>
                    {
                        target.Description = "Liest die für den angegebenen Benutzer verfügbaren Verträge aus.";
                        target.HelpOption("-? | -h | --help");
                    }).OnExecute(() =>
                {
                    InitLogging();
                    return ContractsCommand();
                });

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.Execute(args);
            commandLineApplication.OnExecute(
                () =>
                    {
                        return 0;
                    });

            void InitLogging()
            {
                // Init logging...
                var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };
                if (logLevelOption.HasValue())
                {
                    switch (logLevelOption.Value().ToLowerInvariant())
                    {
                        case "verbose":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                            break;

                        case "debug":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                            break;

                        case "info":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Information;
                            break;

                        case "warning":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                            break;

                        case "error":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Error;
                            break;

                        case "fatal":
                            logLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                            break;
                    }
                }

                if (logFileOption.HasValue())
                {
                    Console.WriteLine("Using log file: {0}", logFileOption.Value());

                    Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(logLevelSwitch)
#if DEBUG
                        .WriteTo.Trace()
#endif
                        .WriteTo.ColoredConsole().WriteTo.File(logFileOption.Value()).CreateLogger();
                }
                else
                {
                    Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(logLevelSwitch)
#if DEBUG
                        .WriteTo.Trace()
#endif
                        .WriteTo.ColoredConsole().CreateLogger();
                }
            }
        }

        private static CommandParameters VerifyParameters()
        {
            var connectParameters = new CommandParameters();

            if (pkcs12file.HasValue())
            {
                if (File.Exists(pkcs12file.Value()))
                {
                    connectParameters.ClientCert = File.ReadAllBytes(pkcs12file.Value());

                    if (password.HasValue())
                    {
                        connectParameters.Password = password.Value();
                    }
                }
                else
                {
                    Log.Error("Specified PKCS#12 file wasn't found: {0}", pkcs12file.Value());
                    return null;
                }
            }
            else
            {
                if (!username.HasValue() || !password.HasValue())
                {
                    Log.Error("No username/password or a client certificate was specified.");
                    return null;
                }

                connectParameters.Username = username.Value();
                connectParameters.Password = password.Value();
            }

            if (!serverId.HasValue())
            {
                Log.Error("No device ID was specified.");
                return null;
            }

            var id = new ServerId(serverId.Value());
            if (!id.IsValid || id.Medium != ObisMedium.Communication)
            {
                Log.Error("Invalid device id: {0}", serverId.Value());
                return null;
            }

            connectParameters.ServerId = serverId.Value();

            if (!address.HasValue())
            {
                Log.Error("No IP address was specified.");
                return null;
            }

            if (!IPAddress.TryParse(address.Value(), out var ip))
            {
                Log.Error("Invalid IP address: {0}", address.Value());
                return null;
            }

            if (!port.HasValue())
            {
                Log.Error("No port was specified.");
                return null;
            }

            if (!ushort.TryParse(port.Value(), out var p))
            {
                Log.Error("Invalid port: {0}", port.Value());
                return null;
            }

            connectParameters.IpEndpoint = new IPEndPoint(ip, p);

            if (timeout.HasValue())
            {
                if (!uint.TryParse(port.Value(), out var t))
                {
                    Log.Error("Invalid timeout: {0}", timeout.Value());
                    return null;
                }
            }

            try
            {
                var hanAdapterInfo = HanAdapterRepository.LoadAdapter(connectParameters.ServerId);
                connectParameters.HanAdapter = hanAdapterInfo.CreateInstance();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load the HAN adapter: {0}", ex.Message);
                return null;
            }

            return connectParameters;
        }

        private static int AdaptersCommand()
        {
            var exportList = new List<AdapterInfo>();

            foreach (var adapter in HanAdapterRepository.AvailableAdapters)
            {
                Console.WriteLine($"{adapter.FlagId}\t{adapter.ManufacturerName}\t{adapter.BaseNamespace}");

                exportList.Add(
                    new AdapterInfo()
                        {
                            FlagId = adapter.FlagId,
                            ManufacturerName = adapter.ManufacturerName,
                            BaseNamespace = adapter.BaseNamespace,
                        });
            }

            if (!exportFile.HasValue())
            {
                return 0;
            }

            var xs = new XmlSerializer(exportList.GetType());
            using (var tw = new StreamWriter(exportFile.Value()))
            {
                xs.Serialize(tw, exportList);
            }

            return 0;
        }

        private static int ContractsCommand()
        {
            var connectParams = VerifyParameters();
            if (connectParams == null)
            {
                return 1;
            }

            var connectResult = Connect(connectParams);
            if (connectResult.error != null)
            {
                Log.Error("Connect failed: {0}", connectResult.error);
                return 2;
            }

            var hanAdapter = connectParams.HanAdapter;

            var contractsResult = hanAdapter.LoadAvailableContracts(
                connectParams.CreateCancellationToken(),
                ProgressCallback).Result;

            if (contractsResult.error != null)
            {
                Log.Error("LoadAvailableContracts failed: {0}", contractsResult.error);
                return 2;
            }

            for (int i = 0; i < contractsResult.contracts.Count; i++)
            {
                var contract = contractsResult.contracts[i];
                Console.WriteLine($"{i} - Contract details:");
                Console.WriteLine($"    TafId:           {contract.TafId}");
                Console.WriteLine($"    TafName:         {contract.TafName}");
                Console.WriteLine($"    SupplierId:      {contract.SupplierId}");
                Console.WriteLine($"    ConsumerId:      {contract.ConsumerId}");
                Console.WriteLine($"    MeteringPointId: {contract.MeteringPointId}");
                Console.WriteLine($"    Begin:           {contract.Begin.ToIso8601Local()}");
                Console.WriteLine($"    End:             {(contract.End?.ToIso8601Local() ?? "-")}");
                Console.WriteLine($"    Meters:          {string.Join(",", contract.Meters)}");

                if (contract.BillingPeriods != null && contract.BillingPeriods.Count > 0)
                {
                    Console.WriteLine("    BillingPeriods:");
                    for (int j = 0; j < contract.BillingPeriods.Count; j++)
                    {
                        var period = contract.BillingPeriods[j];
                        Console.WriteLine(
                            $"      {j} - {period.Begin.ToIso8601Local()} to {(period.End?.ToIso8601Local() ?? "-")}");
                    }
                }
            }

            if (!exportFile.HasValue())
            {
                return 0;
            }

            var contracts = contractsResult.contracts.Select(ci => new Contract(ci)).ToArray();
            var xs = new XmlSerializer(contracts.GetType());
            using (var tw = new StreamWriter(exportFile.Value()))
            {
                xs.Serialize(tw, contracts);
            }
            
            return 0;
        }

        static (ConnectResult result, AdapterError error) Connect(CommandParameters commandParameters)
        {
            var hanAdapter = commandParameters.HanAdapter;

            if (commandParameters.ClientCert != null)
            {
                return hanAdapter.Connect(
                    commandParameters.ServerId,
                    commandParameters.IpEndpoint,
                    commandParameters.ClientCert,
                    commandParameters.Password,
                    new Dictionary<string, string>(),
                    TimeSpan.FromSeconds(30),
                    commandParameters.CreateCancellationToken(),
                    ProgressCallback).Result;
            }

            return hanAdapter.Connect(
                commandParameters.ServerId,
                commandParameters.IpEndpoint,
                commandParameters.Username,
                commandParameters.Password,
                new Dictionary<string, string>(),
                TimeSpan.FromSeconds(30),
                commandParameters.CreateCancellationToken(),
                ProgressCallback).Result;
        }
        
        private static void ProgressCallback(ProgressInfo progressInfo)
        {
            Log.Information("Progress Callback: {@progressInfo}", progressInfo);
        }
    }
}
