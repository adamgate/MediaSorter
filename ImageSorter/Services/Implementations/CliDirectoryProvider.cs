using MediaSorter.Services.Interfaces;

namespace MediaSorter.Services.Implementations
{
    public class CliDirectoryProvider : IDirectoryProvider
    {
        public string? GetValidMediaDirectory(string message)
        {
            Console.WriteLine(message);
            var path = Console.ReadLine();

            if (File.Exists(path))
            {
                Console.WriteLine("{0} is a file and not a directory.", path);
                return null;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("The directory \"{0}\" does not exist.", path);
                return null;
            }

            return path;
        }
    }
}
