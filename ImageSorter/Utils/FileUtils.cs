namespace MediaSorter.Utils
{
    public static class FileUtils 
    {
        public static readonly char[] _illegalFileCharacters = ['/', '\\', '.', '<', '>', ':', '"', '|', '?', '*'];

        public static void CreateDirectoryIfDoesntExist(string path)
        {
            if (Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string? StripIllegalFileCharacters(string fileName)
            => fileName.Where(x => !_illegalFileCharacters.Contains(x)).ToString();

        public static void CopyFile(string sourceFileName, string destFileName)
        {
            try
            {
                File.Copy(sourceFileName, destFileName);
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                    Console.WriteLine("Could not find the source file {0} to copy", sourceFileName);

                else if (ex is DirectoryNotFoundException)
                    Console.WriteLine("Destination folder {0} does not exist for file {1}", Path.GetDirectoryName(destFileName), Path.GetFileName(destFileName));

                else
                {
                    Console.WriteLine("An unexpected error occurred: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
}
