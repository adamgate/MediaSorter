using ImageSorter.Services.Interfaces;

namespace ImageSorter.Services.Implementations
{
    public class CliDirectoryProvider : IDirectoryProvider
    {
        public string? GetValidImageDirectory()
        {
            Console.WriteLine("Please enter the path of the folder you wish to sort:");
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