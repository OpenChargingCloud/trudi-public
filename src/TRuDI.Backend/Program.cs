﻿namespace TRuDI.Backend
{
    using System;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.CommandLineUtils;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using TRuDI.Backend.Application;

#if !DEBUG
    using System;
    using Microsoft.AspNetCore.Server.Kestrel.Https;
    using TRuDI.Backend.Utils;
#endif

    public class Program
    {
        public static CommandLineArguments CommandLineArguments { get; } = new CommandLineArguments();

        private static int backendServerPort;

        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            var logFileOption = commandLineApplication.Option("-l|--log <logfile>", "Datei in der Programmmeldungen protokolliert werden.", CommandOptionType.SingleValue);
            var logLevelOption = commandLineApplication.Option("--loglevel <loglevel>", "Log Level: verbose, debug, info, warning, error, fatal. Standard ist info", CommandOptionType.SingleValue);
            var testFileOption = commandLineApplication.Option("-t|--test <testconfig>", "Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.", CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.Execute(args);

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
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(logLevelSwitch)
#if DEBUG
                    .WriteTo.Trace()
#endif
                    .WriteTo.ColoredConsole()
                    .WriteTo.File(logFileOption.Value())
                    .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(logLevelSwitch)
#if DEBUG
                    .WriteTo.Trace()
#endif
                    .WriteTo.ColoredConsole()
                    .CreateLogger();
            }

            Log.Information(
                "Starting TRuDI {0} on {1}", 
                Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion, 
                System.Runtime.InteropServices.RuntimeInformation.OSDescription);

            if (testFileOption.HasValue())
            {
                // Check for a configuration file/directory used by the example/simulation HAN adapter
                var testConfigFile = testFileOption.Value();
                if (!string.IsNullOrWhiteSpace(testConfigFile)
                    && (File.Exists(testConfigFile) || Directory.Exists(testConfigFile)))
                {
                    Log.Information("Using test configuration file: {0}", testConfigFile);

                    CommandLineArguments.TestConfiguration = testConfigFile;
                    HanAdapterRepository.ActivateExampleHanAdapter();
                }
                else
                {
                    Log.Warning("Configuration file not found: {0}", testConfigFile);
                }
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var webhost = BuildWebHost(args);

            var doneEvent = new ManualResetEventSlim(false);
            using (var cts = new CancellationTokenSource())
            {
                AttachCtrlcSigtermShutdown(cts, doneEvent);

                webhost.Start();

                // If the webhost is listening on the port: send it to the Electron frontend
                Console.WriteLine($"##### TRUDI-BACKEND-PORT: {backendServerPort} #####");

                cts.Token.WaitHandle.WaitOne();
                doneEvent.Set();
            }
        }

        private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent)
        {
            void Shutdown()
            {
                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }

                // Wait on the given reset event
                resetEvent.Wait();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };
        }
        
        public static int FindFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(
                    options =>
                        {
#if DEBUG
                            options.Listen(IPAddress.Loopback, 5000);
                            backendServerPort = 5000;
#else
                            // Get free TCP port and write it to STDOUT where the Electron frontend can catch it.
                            backendServerPort = FindFreeTcpPort();
                            options.Listen(IPAddress.Loopback, backendServerPort, listenOptions =>
                                {
                                    var httpsOptions =
                                        new HttpsConnectionAdapterOptions
                                            {
                                                ServerCertificate =
                                                    CertificateGenerator
                                                        .GenerateCertificate(
                                                            $"CN={DigestUtils.GetDigestFromAssembly(typeof(Program)).ToLowerInvariant()}")
                                            };

                                    listenOptions.UseHttps(httpsOptions);
                                });
#endif
                        })
                .UseStartup<Startup>()
                .Build();
    }
}
