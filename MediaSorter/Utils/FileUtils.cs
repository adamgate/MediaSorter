using MediaSorter.Constants;

namespace MediaSorter.Utils
{
    public static class FileUtils
    {
        public static void CreateDirectoryIfDoesntExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string? StripIllegalFileCharacters(string fileName) =>
            string.Concat(fileName.Where(x => !FileConstants.IllegalFileCharacters.Contains(x)));

        public static void CopyFile(string sourceFileName, string destFileName)
        {
            try
            {
                File.Copy(sourceFileName, destFileName);
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                    Console.WriteLine("Could not find the source file \"{0}\" to copy", sourceFileName);

                else if (ex is DirectoryNotFoundException)
                    Console.WriteLine("Destination folder \"{0}\" does not exist for file \"{1}\"", Path.GetDirectoryName(destFileName), Path.GetFileName(destFileName));

                else if (ex is IOException)
                    Console.WriteLine("Destination file \"{0}\" already exists.", destFileName);

                else
                {
                    Console.WriteLine("An unexpected error occurred: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
}
