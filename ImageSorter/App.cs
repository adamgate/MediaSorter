using MediaSorter.Services.Interfaces;

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
        private readonly IMediaScanner _mediaProcessor;

        public App(
            IDirectoryProvider directoryProvider,
            IFileSorter fileSorter,
            IMetadataProvider metadataProvider,
            IMediaScanner mediaScanner
        )
        {
            _directoryProvider = directoryProvider;
            _fileSorter = fileSorter;
            _metadataProvider = metadataProvider;
            _mediaProcessor = mediaScanner;
        }

        public int Run(string[] args)
        {
            try
            {
                string? readDirectory = "";
                while (string.IsNullOrEmpty(readDirectory))
                    readDirectory = _directoryProvider.GetValidMediaDirectory(
                        "Please enter the path of the folder you wish to sort:"
                    );

                var mediaPaths = _mediaProcessor.GetMediaInPath(readDirectory);
                var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);

                string? writeDirectory = "";
                while (string.IsNullOrEmpty(readDirectory))
                    readDirectory = _directoryProvider.GetValidMediaDirectory(
                        "\"Please enter the path of the folder where you wish to save the sorted files:\""
                    );

                _fileSorter.SortMediaFilesByDate(writeDirectory, mediaWithMetadata);

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
