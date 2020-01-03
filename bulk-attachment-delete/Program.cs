using CommandLine;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace bulk_attachment_delete
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private static void Main(string[] args)
        {
            var enviroment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(enviroment) || enviroment.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (isDevelopment)
            {
                builder.AddUserSecrets<GivingSecrets>();
            }

            Configuration = builder.Build();

            var services = new ServiceCollection()
               .AddOptions()
               .Configure<GivingSecrets>(Configuration.GetSection(nameof(GivingSecrets)))
               .AddSingleton<IAppHost, AppHost>()
               .AddSingleton<IGivingApiService, GivingApiService>()
               .BuildServiceProvider();

            var appHost = services.GetService<IAppHost>();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(appHost.Run)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.WriteLine("Version Request");
                return;
            }

            if (errs.IsHelp())
            {
                Console.WriteLine("Help Request");
                return;
            }

            Console.WriteLine("Parser Fail:");

            foreach (Error err in errs)
            {
                Console.WriteLine(err.ToString());
            }            
        }
    }
}
