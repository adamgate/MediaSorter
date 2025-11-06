using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

namespace MediaSorter.Services.Implementations
{
    // TODO - make class static? Currently has no fields/properties
    /// <summary>
    /// Scans for all the
    /// </summary>
    public class MediaScanner : IMediaScanner
    {
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
                    mediaPaths.Add(filePath);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                mediaPaths.AddRange(GetMediaPathsRecursively(subdirectory));

            return mediaPaths;
        }

        private static bool ShouldProcessFile(string path)
        {
            var fileExtension = Path.GetExtension(path);

            return Constants.Constants.SupportedMediaFormats.Any(
                x => x.EqualsIgnoreCase(fileExtension)
            );
        }
    }
}
