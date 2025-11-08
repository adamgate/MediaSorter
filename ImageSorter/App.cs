using MediaSorter.Constants;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

namespace MediaSorter
{
    /// <summary>
    ///  Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDirectoryProvider _directoryProvider;
        private readonly IFileSorter _fileSorter;
        private readonly IMetadataProvider _metadataProvider;
        private readonly IMediaScanner _mediaScanner;

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

        public int Run(string[] args)
        {
            try
            {
                var readDirectory = _directoryProvider.GetValidDirectory("Please enter the path of the folder you wish to sort:");
                if (readDirectory is null)
                {
                    Console.WriteLine("Exiting...");
                    return Environment.ExitCode = 0;
                }

                var mediaPaths = _mediaScanner.GetMediaInPath(readDirectory);

                var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
                if (mediaWithMetadata.Count == 0)
                {
                    Console.WriteLine("No media files were found. Exiting...");
                    return Environment.ExitCode = 0;
                }

                Console.WriteLine("Found {0} media files.", mediaWithMetadata.Count);
                var shouldProceed = CliUtils.GetYesNoFromUser("Are you sure you want to proceed? (Y/N)");
                if (!shouldProceed)
                {
                    Console.WriteLine("Exiting...");
                    return Environment.ExitCode = 0;
                }

                var writeDirectory = _directoryProvider.GetValidDirectory("Please enter the path of the folder where you wish to save the sorted files:");
                if (writeDirectory is null)
                {
                    Console.WriteLine("Exiting...");
                    return Environment.ExitCode = 0;
                }

                _fileSorter.SortMediaFilesByDate(writeDirectory, mediaWithMetadata);

                Console.WriteLine("Successfully sorted {0} files. Exiting...", mediaWithMetadata.Count);
                return Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
                return Environment.ExitCode = 1;
            }
        }
    }
}
