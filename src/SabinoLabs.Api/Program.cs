#nullable enable
using System;
using System.IO;
using System.Security.Authentication;
using JHipsterNet.Web.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Syslog;
using Winton.Extensions.Configuration.Consul;
using static JHipsterNet.Core.Boot.BannerPrinter;

namespace SabinoLabs
{
    public class Program
    {
        private const string SerilogSection = "Serilog";
        private const string SyslogPort = "SyslogPort";
        private const string SyslogUrl = "SyslogUrl";
        private const string SyslogAppName = "SyslogAppName";
        private const string ConsulSection = "Consul";
        private const string ConsulHost = "Host";

        public static int Main(string[] args)
        {
            PrintBanner(10 * 1000);

            try
            {
                IConfiguration appConfiguration = GetAppConfiguration();
                Log.Logger = CreateLogger(appConfiguration);

                CreateWebHostBuilder(args)
                    .ConfigureAppConfiguration(
                        builder =>
                            builder.AddConsul(
                                    "config/application/data",
                                    options =>
                                    {
                                        options.ConsulConfigurationOptions =
                                            cco =>
                                            {
                                                cco.Address =
                                                    new Uri(appConfiguration.GetSection(ConsulSection)[ConsulHost]);
                                            };
                                        //options.Optional = true;
                                        //options.PollWaitTime = TimeSpan.FromSeconds(5);
                                        //options.ReloadOnChange = true;
                                    })
                                .AddEnvironmentVariables())
                    .Build()
                    .Run();

                return 0;
            }
            catch (Exception ex)
            {
                // Use ForContext to give a context to this static environment (for Serilog LoggerNameEnricher).
                Log.ForContext<Program>().Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(params string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();

        /// <summary>
        ///     Create application logger from configuration.
        /// </summary>
        /// <returns></returns>
        private static ILogger CreateLogger(IConfiguration appConfiguration)
        {
            int port = 6514;

            if (appConfiguration.GetSection(SerilogSection)[SyslogPort] != null)
            {
                if (int.TryParse(appConfiguration.GetSection(SerilogSection)[SyslogPort], out int portFromConf))
                {
                    port = portFromConf;
                }
            }

            string url = appConfiguration.GetSection(SerilogSection)[SyslogUrl] != null
                ? appConfiguration.GetSection(SerilogSection)[SyslogUrl]
                : "localhost";
            string appName = appConfiguration.GetSection(SerilogSection)[SyslogAppName] != null
                ? appConfiguration.GetSection(SerilogSection)[SyslogAppName]
                : "SabinoLabsApp";
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .Enrich.With<LoggerNameEnricher>()
                .WriteTo.TcpSyslog(url, port, appName, FramingType.OCTET_COUNTING, SyslogFormat.RFC5424,
                    Facility.Local0, SslProtocols.None)
                .ReadFrom.Configuration(appConfiguration);

            return loggerConfiguration.CreateLogger();
        }

        /// <summary>
        ///     Gets the current application configuration
        ///     from global and specific appsettings.
        /// </summary>
        /// <returns>Return the application <see cref="IConfiguration" /></returns>
        private static IConfiguration GetAppConfiguration()
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
