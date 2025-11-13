using MediaSorter.Services.Implementations;
using MediaSorter.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            builder.Services.AddScoped<IMediaScanner, MediaScanner>();
            builder.Services.AddScoped<IDirectoryProvider, CliDirectoryProvider>();
            builder.Services.AddScoped<IMetadataProvider, MetadataProvider>();
            builder.Services.AddScoped<IFileSorter, FileSorter>();

            builder.Services.AddTransient<App>();

            var app = builder.Build();
            app.Services.GetRequiredService<App>().Run(args);
        }
    }
}