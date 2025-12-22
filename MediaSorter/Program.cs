using MediaSorter.Services.Implementations;
using MediaSorter.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Spectre.Console;

namespace MediaSorter
{
    /// <summary>
    /// Sets up configuration and runs the app.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging(builder =>
            {
                var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log.txt")
                .CreateLogger();

                builder.AddSerilog(logger); // register serilog as a logging provider
            });

            builder.Services.AddSingleton(AnsiConsole.Console);

            builder.Services.AddScoped<IDateParser, DateParser>();
            builder.Services.AddScoped<IDirectoryProvider, CliDirectoryProvider>();
            builder.Services.AddScoped<IFileSorter, FileSorter>();
            builder.Services.AddScoped<IMediaScanner, MediaScanner>();
            builder.Services.AddScoped<IMetadataProvider, MetadataProvider>();

            builder.Services.AddTransient<App>();

            var app = builder.Build();

            app.Services.GetRequiredService<App>().Run(args);
        }
    }
}