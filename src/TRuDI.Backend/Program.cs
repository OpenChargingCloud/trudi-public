namespace TRuDI.Backend
{
    using System.IO;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.CommandLineUtils;

    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    using TRuDI.Backend.Application;

    public class Program
    {
        public static CommandLineArguments CommandLineArguments { get; } = new CommandLineArguments();

        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            var logFileOption = commandLineApplication.Option("-l|--log <logfile>", "Datei in der Programmmeldungen protokolliert werden.", CommandOptionType.SingleValue);
            var logLevelOption = commandLineApplication.Option("--loglevel <loglevel>", "Log Level: verbose, debug, info, warning, error, fatal. Standard ist info", CommandOptionType.SingleValue);
            var testFileOption = commandLineApplication.Option("-t|--test <testconfig>", "Aktiviert den Test-HAN-Adapter mit der angegebenen Konfigurationsdatei.", CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.Execute(args);

            var logLevelSwitch = new LoggingLevelSwitch();
            logLevelSwitch.MinimumLevel = LogEventLevel.Information;

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

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
/*                .UseKestrel(
                    options =>
                        {
                            options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                                {
                                    listenOptions.UseHttps("testCert.pfx", "testPassword");
                                });
                        })*/
                .UseStartup<Startup>()
                .Build();
    }
}
