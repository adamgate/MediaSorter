using MediaSorter.Constants;

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
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Prints a message in the provided foreground color and then exits the program with the provided exit code.
        /// </summary>
        public static void DisplayMessageAndExit(string message, ConsoleColor foregroundColor, int exitCode)
        {
            DisplayMessageWithColor(message, foregroundColor);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Prints a message in the provided foreground color, then reverts the color back to the original.
        /// </summary>
        public static void DisplayMessageWithColor(string message, ConsoleColor foregroundColor)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor; ;
        }

        /// <summary>
        /// Loops until the user chooses yes or no.
        /// </summary>
        public static bool GetYesNoFromUser(string message)
        {
            string? input = null;
            while (input == null || (!CommandLineConstants.ConfirmationCommands.ContainsIgnoreCase(input) && !CommandLineConstants.DeclineCommands.ContainsIgnoreCase(input)))
            {
                Console.WriteLine(message);
                Console.Write("> ");
                input = Console.ReadLine();
            }

            return CommandLineConstants.ConfirmationCommands.ContainsIgnoreCase(input);
        }
    }
}