using MediaSorter.Constants;

namespace MediaSorter.Utils
{
    /// <summary>
    /// Utilities related to the file system.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Attempts to copy a file to the provided destination.
        /// </summary>
        public static void CopyFile(string sourceFileName, string destinationFileName)
        {
            try
            {
                File.Copy(sourceFileName, destinationFileName);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Empty;

                if (ex is FileNotFoundException)
                    errorMessage = $"Could not find the source file \"{sourceFileName}\" to copy";
                else if (ex is DirectoryNotFoundException)
                    errorMessage = $"Destination folder \"{Path.GetDirectoryName(destinationFileName)}\" does not exist for file \"{Path.GetFileName(destinationFileName)}\"";
                else if (ex is IOException)
                    errorMessage = $"Destination file \"{destinationFileName}\" already exists.";
                else
                {
                    errorMessage = $"An unexpected error occurred: {ex.Message}";
                    CliUtils.DisplayMessageWithColor(errorMessage, ConsoleColor.Red);
                    throw;
                }

                CliUtils.DisplayMessageWithColor(errorMessage, ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Creates a directory if it doesn't already exist.
        /// </summary>
        public static void CreateDirectoryIfDoesntExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Strips illegal file characters from the provided string.
        /// </summary>
        public static string? StripIllegalFileCharacters(string fileName) =>
            string.Concat(fileName.Where(x => !FileConstants.IllegalFileCharacters.Contains(x)));
    }
}