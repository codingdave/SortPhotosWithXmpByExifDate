using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers;

namespace SortPhotosWithXmpByExifDateCli;

public static class Configuration
{
    public static IHost CreateHost()
    {
        var appsettings = GetBasePath() + "appsettings.json";
        var additionalAppsettings = GetBasePath() + $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json";
        
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(GetBasePath())
            .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
            .AddJsonFile(additionalAppsettings, optional: true, reloadOnChange: true)
            ;

        var configurationRoot = configurationBuilder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configurationRoot)
            .Enrich.FromLogContext()
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(serviceCollection => { })
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                _ = configurationBuilder
                    .SetBasePath(GetBasePath())
                    .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
                    .AddJsonFile(path: additionalAppsettings, optional: true);
            })
            .ConfigureLogging(loggingBuilder =>
            {
                // _ = loggingBuilder.ClearProviders().AddConfiguration()
            })
            .UseSerilog()
            .Build();

        Log.Information($"reading configuration from required '{appsettings}' and optional '{additionalAppsettings}'");
        return host;
    }

    public static string GetBasePath()
    {
        // using var processModule = Process.GetCurrentProcess().MainModule;
        // return Path.GetDirectoryName(processModule?.FileName) ?? string.Empty;
        // return Directory.GetCurrentDirectory();
        // return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        return AppContext.BaseDirectory;
    }
}
