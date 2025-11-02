using ImageSorter.Services.Implementations;
using ImageSorter.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageSorter
{
    /// <summary>
    /// Sets up configuration and runs the app.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Hosting and DI are overkill for this app, but good for practice
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddScoped<IFileProcessor, FileProcessor>();
            builder.Services.AddScoped<IMetadataProvider, MetadataProvider>();
            builder.Services.AddScoped<IDirectoryProvider, CliDirectoryProvider>();

            builder.Services.AddTransient<App>();

            var app = builder.Build();
            app.Services.GetRequiredService<App>().Run(args);
        }
    }
}