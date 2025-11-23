using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using System.Reflection;
using System.Text;

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
        private readonly IMediaScanner _mediaScanner;
        private readonly IMetadataProvider _metadataProvider;
        private readonly string _version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "X.X.X.X";

        public App(
            IDateParser dateParser,
            IDirectoryProvider directoryProvider,
            IFileSorter fileSorter,
            IMediaScanner mediaScanner,
            IMetadataProvider metadataProvider)
        {
            _dateParser = dateParser;
            _directoryProvider = directoryProvider;
            _fileSorter = fileSorter;
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
                    CliUtils.DisplayMessageAndExit("The output directory cannot be the same as the source directory. Exiting...", ConsoleColor.Yellow, 0);

                var shouldProceed = CliUtils.GetYesNoFromUser("\nAre you sure you want to proceed? (Y/N)");
                if (!shouldProceed)
                    CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);

                var mediaWithProcessedDates = ParseMediaDatesTaken(mediaWithMetadata);
                SortMediaFiles(outputDirectory, mediaWithProcessedDates);
            }
            catch (Exception ex)
            {
                CliUtils.DisplayMessageAndExit($"An error occurred: {ex.Message}", ConsoleColor.Red, 1);
            }
        }

        private IEnumerable<string> GetMediaPaths(string sourceDirectory)
        {
            Console.WriteLine("\nScanning for media...");
            var mediaPaths = _mediaScanner.GetMediaInPath(sourceDirectory);
            if (!mediaPaths.Any())
                CliUtils.DisplayMessageAndExit("No media files were found. Exiting...", ConsoleColor.Yellow, 0);
            Console.WriteLine("Found {0} media files.", mediaPaths.Count());

            return mediaPaths;
        }

        private string GetOutputDirectory()
        {
            var outputDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder where you wish to save the sorted files:");
            if (outputDirectory is null)
                CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);

            return outputDirectory;
        }

        private string GetSourceDirectory()
        {
            var sourceDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder you wish to sort:");
            if (sourceDirectory is null)
                CliUtils.DisplayMessageAndExit("Exiting...", ConsoleColor.Yellow, 0);

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
            var mediaWithProcessedDates = _dateParser.Parse(mediaWithMetadata);
            Console.WriteLine("Done processing dates.");

            return mediaWithProcessedDates;
        }

        private void SortMediaFiles(string outputDirectory, IDictionary<string, DateMetadata> mediaWithProcessedDates)
        {
            Console.WriteLine($"\nSorting {mediaWithProcessedDates.Count} files...");
            _fileSorter.SortMediaFilesByDate(outputDirectory, mediaWithProcessedDates);
            CliUtils.DisplayMessageAndExit($"\nSuccessfully sorted {mediaWithProcessedDates.Count} files. Exiting...", ConsoleColor.Green, 0);
        }
    }
}