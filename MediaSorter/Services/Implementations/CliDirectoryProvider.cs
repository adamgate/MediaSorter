using MediaSorter.Constants;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Provides valid directories through the CLI.
    /// </summary>
    public class CliDirectoryProvider : IDirectoryProvider
    {
        /// <summary>
        /// Attempts to get a valid directory through the CLI.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        /// <returns>A <c>string</c> representing an existing directory or <c>null</c>.</returns>
        public string? GetValidDirectory(string message)
        {
            string? directory = "";
            while (string.IsNullOrEmpty(directory))
                directory = LoopUntilAcceptableInput(message);

            if (CommandLineConstants.TerminationCommands.Contains(directory))
                return null;

            return directory;
        }

        private static string? LoopUntilAcceptableInput(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("(You can type \"exit\" to close the app)");
            Console.Write("> ");
            var path = Console.ReadLine();

            if (CommandLineConstants.TerminationCommands.Contains(path))
                return path;

            if (File.Exists(path))
            {
                var tempMessage = string.Format("\"{0}\" is a file and not a directory. Please choose an existing directory.", path);
                CliUtils.DisplayMessageWithColor(tempMessage, ConsoleColor.Red);
                return null;
            }

            if (!Directory.Exists(path))
            {
                var tempMessage = string.Format("The directory \"{0}\" does not exist. Please choose an existing directory.", path);
                CliUtils.DisplayMessageWithColor(tempMessage, ConsoleColor.Red);
                return null;
            }

            return path;
        }
    }
}