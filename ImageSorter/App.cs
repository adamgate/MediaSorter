using MediaSorter.Services.Interfaces;

namespace MediaSorter
{
    /// <summary>
    ///  Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDirectoryProvider _directoryProvider;
        private readonly IMetadataProvider _metadataProvider;
        private IMediaScanner _mediaProcessor;

        public App(
            IDirectoryProvider directoryProvider,
            IMetadataProvider metadataProvider,
            IMediaScanner fileProcessor
        )
        {
            _directoryProvider = directoryProvider;
            _metadataProvider = metadataProvider;
            _mediaProcessor = fileProcessor;
        }

        public int Run(string[] args)
        {
            try
            {
                string? workDirectory = "";
                while (string.IsNullOrEmpty(workDirectory))
                    workDirectory = _directoryProvider.GetValidMediaDirectory();

                var mediaPaths = _mediaProcessor.GetMediaInPath(workDirectory);
                var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
                // TODO -
                // _fileUtils.Save();

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
