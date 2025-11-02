using ImageSorter.Services.Interfaces;
using ImageSorter.Utils;

namespace ImageSorter.Services.Implementations
{
    // TODO - make class static? Currently has no fields/properties
    public class FileProcessor : IFileProcessor
    {
        public IEnumerable<string> GetImagesInPath(string path)
        {
            return GetImagePathsRecursively(path);
        }

        private static IEnumerable<string> GetImagePathsRecursively(string targetDirectory)
        {
            var imagePaths = new List<string>();

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string filePath in fileEntries)
            {
                if (ShouldProcessFile(filePath))
                    imagePaths.Add(filePath);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                imagePaths.AddRange(GetImagePathsRecursively(subdirectory));

            return imagePaths;
        }

        private static bool ShouldProcessFile(string path)
        {
            var fileExtension = Path.GetExtension(path);
            return Constants.Constants.SupportedImageFormats.Any(x => x.EqualsIgnoreCase(fileExtension));
        }
    }
}