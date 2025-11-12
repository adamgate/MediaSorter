using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using System.Text;

namespace MediaSorter
{
    /// <summary>
    ///  Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDirectoryProvider _directoryProvider;
        private readonly IFileSorter _fileSorter;
        private readonly IMediaScanner _mediaScanner;
        private readonly IMetadataProvider _metadataProvider;

        public App(
            IDirectoryProvider directoryProvider,
            IFileSorter fileSorter,
            IMetadataProvider metadataProvider,
            IMediaScanner mediaScanner)
        {
            _directoryProvider = directoryProvider;
            _fileSorter = fileSorter;
            _metadataProvider = metadataProvider;
            _mediaScanner = mediaScanner;
        }

        public void Run(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine($"MEDIA SORTER v0.8.0 \n© {DateTime.Now.ToString("MMMM yyyy")}");
                Console.WriteLine("-------------------------------");

                var sourceDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder you wish to sort:");
                if (sourceDirectory is null)
                    CliUtils.DisplayMessageAndExit("Exiting...", 0);

                Console.WriteLine("\nScanning for media...");
                var mediaPaths = _mediaScanner.GetMediaInPath(sourceDirectory);

                var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
                if (mediaWithMetadata.Count == 0)
                    CliUtils.DisplayMessageAndExit("No media files were found. Exiting...", 0);

                var outputDirectory = _directoryProvider.GetValidDirectory("\nPlease enter the path of the folder where you wish to save the sorted files:");
                if (outputDirectory is null)
                    CliUtils.DisplayMessageAndExit("Exiting...", 0);
                if (outputDirectory.Equals(sourceDirectory))
                    CliUtils.DisplayMessageAndExit("The output directory cannot be the same as the source directory. Exiting...", 0);

                Console.WriteLine("Found {0} media files.", mediaWithMetadata.Count);
                var shouldProceed = CliUtils.GetYesNoFromUser("Are you sure you want to proceed? (Y/N)");
                if (!shouldProceed)
                    CliUtils.DisplayMessageAndExit("Exiting...", 0);

                Console.WriteLine($"\nSorting {mediaWithMetadata.Count} files...");
                _fileSorter.SortMediaFilesByDate(outputDirectory, mediaWithMetadata);

                CliUtils.DisplayMessageAndExit($"Successfully sorted {mediaWithMetadata.Count} files. Exiting...", 0);
            }
            catch (Exception ex)
            {
                CliUtils.DisplayMessageAndExit($"An error occurred: {ex.Message}", 1);
            }
        }
    }
}