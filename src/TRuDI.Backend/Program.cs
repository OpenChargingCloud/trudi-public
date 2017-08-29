namespace TRuDI.Backend
{
    using System.IO;
    using System.Linq;
    using System.Net;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    using TRuDI.Backend.Application;

    public class Program
    {
        public static CommandLineArguments CommandLineArguments { get; } = new CommandLineArguments();

        public static void Main(string[] args)
        {
            // Check for a configuration file/directory used by the example/simulation HAN adapter
            var testConfigFile = args.FirstOrDefault(a => a.StartsWith("--test="))?.Substring(7);
            if (!string.IsNullOrWhiteSpace(testConfigFile) && (File.Exists(testConfigFile) || Directory.Exists(testConfigFile)))
            {
                CommandLineArguments.TestConfiguration = testConfigFile;
                HanAdapterRepository.ActivateExampleHanAdapter();
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
