using MediaSorter.Services.Interfaces;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Provides valid directories through the CLI.
    /// </summary>
    public class CliDirectoryProvider : IDirectoryProvider
    {
        /// <summary>
        /// Attempts to get a valid directory via the CLI.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string? GetValidDirectory(string message)
        {
            Console.WriteLine(message);
            var path = Console.ReadLine();

            if (File.Exists(path))
            {
                Console.WriteLine("\"{0}\" is a file and not a directory. Please choose an existing directory.", path);
                return null;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("The directory \"{0}\" does not exist. Please choose an existing directory.", path);
                return null;
            }

            return path;
        }
    }
}
