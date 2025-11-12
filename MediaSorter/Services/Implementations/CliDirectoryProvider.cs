using MediaSorter.Constants;
using MediaSorter.Services.Interfaces;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Provides valid directories through the CLI.
    /// </summary>
    public class CliDirectoryProvider : IDirectoryProvider
    {
        public string? GetValidDirectory(string message)
        {
            string? directory = "";
            while (string.IsNullOrEmpty(directory))
                directory = LoopUntilAcceptableInput(message);

            if (AppConstants.TerminationCommands.Contains(directory))
                return null;

            return directory;
        }

        private static string? LoopUntilAcceptableInput(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("(You can type \"exit\" to close the app)");
            Console.Write("> ");
            var path = Console.ReadLine();

            if (AppConstants.TerminationCommands.Contains(path))
                return path;

            if (File.Exists(path))
            {
                Console.WriteLine("\"{0}\" is a file and not a directory. Please choose an existing directory.\n", path);
                return null;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("The directory \"{0}\" does not exist. Please choose an existing directory.\n", path);
                return null;
            }

            return path;
        }
    }
}
