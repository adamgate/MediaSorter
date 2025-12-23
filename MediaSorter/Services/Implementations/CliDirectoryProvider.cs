using MediaSorter.Constants;
using MediaSorter.Services.Interfaces;

using Spectre.Console;

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
        public string? GetValidDirectory(string message)
        {
            string? directory = "";
            while (string.IsNullOrEmpty(directory))
            {
                directory = LoopUntilAcceptableInput(message);
            }

            if (CommandLineConstants.TerminationCommands.Contains(directory))
            {
                return null;
            }

            return directory;
        }

        private static string? LoopUntilAcceptableInput(string message)
        {
            var path = AnsiConsole.Prompt(
                new TextPrompt<string>(message)
            );

            if (CommandLineConstants.TerminationCommands.Contains(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                AnsiConsole.MarkupLine($"[red]\"{path}\" is a file and not a directory. Please choose an existing directory.[/]");
                //var tempMessage = string.Format("\"{0}\" is a file and not a directory. Please choose an existing directory.", path);
                //CliUtils.DisplayMessageWithColor(tempMessage, ConsoleColor.Red);
                return null;
            }

            if (!Directory.Exists(path))
            {
                AnsiConsole.MarkupLine($"[red]The directory \"{path}\" does not exist. Please choose an existing directory.[/]");
                //var tempMessage = string.Format("The directory \"{0}\" does not exist. Please choose an existing directory.", path);
                //CliUtils.DisplayMessageWithColor(tempMessage, ConsoleColor.Red);
                return null;
            }

            return path;
        }
    }
}