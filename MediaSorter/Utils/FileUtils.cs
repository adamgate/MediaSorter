using System.Text;

namespace MediaSorter.Utils
{
    /// <summary>
    /// Utilities related to the file system.
    /// </summary>
    public static class FileUtils
    {
        // TODO - move to File Service for testing & logging functionality
        /// <summary>
        /// Attempts to copy a file to the provided destination.
        /// </summary>
        public static (bool, string) CopyFile(string sourceFileName, string destinationFileName)
        {
            try
            {
                File.Copy(sourceFileName, destinationFileName);
            }
            catch (Exception ex)
            {
                string errorMessage;

                if (ex is FileNotFoundException)
                {
                    errorMessage = $"Could not find the source file \"{sourceFileName}\" to copy";
                }
                else if (ex is DirectoryNotFoundException)
                {
                    errorMessage = $"Destination folder \"{Path.GetDirectoryName(destinationFileName)}\" does not exist for file \"{Path.GetFileName(destinationFileName)}\"";
                }
                else if (ex is IOException)
                {
                    errorMessage = $"Destination file \"{destinationFileName}\" already exists.";
                }
                else
                {
                    errorMessage = $"An unexpected error occurred: {ex.Message}";
                    CliUtils.DisplayMessageWithColor(errorMessage, "red");
                    throw;
                }

                return new(false, errorMessage);
            }

            return new(true, $"Copied to {destinationFileName}");
        }

        // TODO - move to File Service for testing & logging functionality
        /// <summary>
        /// Creates a directory if it doesn't already exist.
        /// </summary>
        public static void CreateDirectoryIfDoesntExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Strips illegal characters from the provided string.
        /// </summary>
        public static string StripIllegalCharacters(this string fileName, IEnumerable<char> illegalCharacters)
        {
            var stringBuilder = new StringBuilder(fileName);

            foreach (var character in illegalCharacters)
            {
                stringBuilder.Replace(character.ToString(), "");
            }

            return stringBuilder.ToString();
        }
    }
}