using ImageSorter.Services.Interfaces;

namespace ImageSorter
{
    /// <summary>
    ///  Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDirectoryProvider _imageDirectoryProvider;
        private readonly IMetadataProvider _metadataProvider;
        private IFileProcessor _fileProcessor;

        public App(
            IDirectoryProvider imageDirectoryProvider,
            IMetadataProvider metadataProvider,
            IFileProcessor fileProcessor)
        {
            _imageDirectoryProvider = imageDirectoryProvider;
            _metadataProvider = metadataProvider;
            _fileProcessor = fileProcessor;
        }

        public int Run(string[] args)
        {
            try
            {
                // TODO - remove this and actually get info from _imageDirectoryProvider
                string? workDirectory = @"C:\Users\Adam\Downloads";
                while (workDirectory is null)
                    _imageDirectoryProvider.GetValidImageDirectory();

                var images = _fileProcessor.GetImagesInPath(workDirectory);
                var imagesWithMetadata = _metadataProvider.GetDateTakenMetadata(images);
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