using System.Reflection;
using System.Text;

using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

using Microsoft.Extensions.Logging;

namespace MediaSorter
{
    /// <summary>
    /// Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDateParser _dateParser;
        private readonly IDirectoryProvider _directoryProvider;
        private readonly IFileSorter _fileSorter;
        private readonly ILogger<App> _logger;
        private readonly IMediaScanner _mediaScanner;
        private readonly IMetadataProvider _metadataProvider;
        private readonly string _version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "X.X.X.X";

        public App(
            IDateParser dateParser,
            IDirectoryProvider directoryProvider,
            IFileSorter fileSorter,
            ILogger<App> logger,
            IMediaScanner mediaScanner,
            IMetadataProvider metadataProvider)
        {
            _dateParser = dateParser;
            _directoryProvider = directoryProvider;
            _fileSorter = fileSorter;
            _logger = logger;
            _metadataProvider = metadataProvider;
            _mediaScanner = mediaScanner;
        }

        public void Run(string[] args)
        {
            // Allow copyright symbol to display correctly
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine($"MEDIA SORTER v{_version}");
                Console.WriteLine($"© {DateTime.Now:MMMM yyyy}");
                Console.WriteLine("-------------------------------");

                var sourceDirectory = GetSourceDirectory();
                var mediaPaths = GetMediaPaths(sourceDirectory);
                var mediaWithMetadata = LoadMediaMetadata(mediaPaths);

                var outputDirectory = GetOutputDirectory();
                if (outputDirectory.Equals(sourceDirectory))
                {
                    CliUtils.DisplayMessageAndExit("The output folder cannot be the same as the source folder. Exiting...", ConsoleColor.Yellow, 0);
                }

                var shouldProceed = CliUtils.GetYesNoFromUser("\nDo you want to proceed? (Y/N)");
                if (!shouldProceed)
                {
                    CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);
                }

                var mediaWithDatesTaken = ParseMediaDatesTaken(mediaWithMetadata);
                SortMediaFiles(outputDirectory, mediaWithDatesTaken);
                _logger.LogDebug("Done sorting {count} files.", mediaWithDatesTaken.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                CliUtils.DisplayMessageWithColor($"An error occurred: {ex.Message}", ConsoleColor.Red);
                CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Red, 1);
            }
        }

        private IEnumerable<string> GetMediaPaths(string sourceDirectory)
        {
            Console.WriteLine("\nScanning for media...");
            var mediaPaths = _mediaScanner.GetMediaInPath(sourceDirectory);
            if (!mediaPaths.Any())
            {
                CliUtils.DisplayMessageAndExit("No media files were found. Exiting...", ConsoleColor.Yellow, 0);
            }

            Console.WriteLine("Found {0} media files.", mediaPaths.Count());
            _logger.LogDebug("Found {count} media file(s) to sort.", mediaPaths.Count());

            return mediaPaths;
        }

        private string GetOutputDirectory()
        {
            var outputDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder where you wish to save the sorted files:");
            if (outputDirectory is null)
            {
                CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);
            }

            _logger.LogDebug("Output Directory: \"{directory}\"", outputDirectory);

            return outputDirectory;
        }

        private string GetSourceDirectory()
        {
            var sourceDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder you wish to sort:");
            if (sourceDirectory is null)
            {
                CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);
            }

            _logger.LogDebug("Source Directory: \"{directory}\"", sourceDirectory);

            return sourceDirectory;
        }

        private IDictionary<string, IEnumerable<RawMetadata>> LoadMediaMetadata(IEnumerable<string> mediaPaths)
        {
            Console.WriteLine("\nLoading date metadata for {0} files...", mediaPaths.Count());
            var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
            Console.WriteLine("Date metadata loaded.");

            return mediaWithMetadata;
        }

        private IDictionary<string, DateMetadata> ParseMediaDatesTaken(IDictionary<string, IEnumerable<RawMetadata>> mediaWithMetadata)
        {
            Console.WriteLine("\nProcessing dates...");
            var mediaWithDatesTaken = _dateParser.Parse(mediaWithMetadata);
            Console.WriteLine("Done processing dates.");

            return mediaWithDatesTaken;
        }

        private void SortMediaFiles(string outputDirectory, IDictionary<string, DateMetadata> mediaWithDatesTaken)
        {
            Console.WriteLine($"\nSorting {mediaWithDatesTaken.Count} files...");
            _fileSorter.SortMediaFilesByDate(outputDirectory, mediaWithDatesTaken);
            CliUtils.DisplayMessageAndExit($"\nSuccessfully sorted {mediaWithDatesTaken.Count} files. Exiting...", ConsoleColor.Green, 0);
        }
    }
}