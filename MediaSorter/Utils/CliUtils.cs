using Spectre.Console;

namespace MediaSorter.Utils
{
    /// <summary>
    /// Utilities related to the command line.
    /// </summary>
    public static class CliUtils
    {
        /// <summary>
        /// Prints a message and then exits the program with the provided exit code.
        /// </summary>
        public static void DisplayMessageAndExit(string message, int exitCode)
        {
            AnsiConsole.MarkupLine(message);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Prints a message in the provided foreground color and then exits the program with the provided exit code.
        /// </summary>
        public static void DisplayMessageAndExit(string message, string foregroundColor, int exitCode)
        {
            DisplayMessageWithColor(message, foregroundColor);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Prints a message in the provided foreground color, then reverts the color back to the original.
        /// </summary>
        public static void DisplayMessageWithColor(string message, string foregroundColor)
        {
            AnsiConsole.MarkupLine($"[{foregroundColor}]{message}[/]");
        }

        /// <summary>
        /// Loops until the user chooses yes or no.
        /// </summary>
        public static bool GetYesNoFromUser(string message)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                    .Title(message)
                    .AddChoices(["Yes", "No"]);
            selectionPrompt.HighlightStyle("bold orange1");

            var choice = AnsiConsole.Prompt(selectionPrompt);

            return choice.EqualsIgnoreCase("Yes");
        }
    }
}