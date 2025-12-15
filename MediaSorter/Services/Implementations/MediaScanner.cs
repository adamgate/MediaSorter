using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Scans for valid media files.
    /// </summary>
    public class MediaScanner : IMediaScanner
    {
        /// <summary>
        /// Gets all supported media paths in the provided directory.
        /// </summary>
        public IEnumerable<string> GetMediaInPath(string path)
        {
            return GetMediaPathsRecursively(path);
        }

        private static IEnumerable<string> GetMediaPathsRecursively(string targetDirectory)
        {
            var mediaPaths = new List<string>();

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string filePath in fileEntries)
            {
                if (ShouldProcessFile(filePath))
                {
                    mediaPaths.Add(filePath);
                }
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                mediaPaths.AddRange(GetMediaPathsRecursively(subdirectory));
            }

            return mediaPaths;
        }

        private static bool ShouldProcessFile(string path)
        {
            var fileExtension = Path.GetExtension(path);

            return Constants.FileConstants.SupportedMediaFormats.Any(x => x.EqualsIgnoreCase(fileExtension));
        }
    }
}