using MediaSorter.Constants;

namespace MediaSorter.Utils
{
    public static class CliUtils
    {
        public static void DisplayMessageAndExit(string message, int exitCode)
        {
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        public static bool GetYesNoFromUser(string message)
        {
            string? input = null;
            while (input == null || (!AppConstants.ConfirmationCommands.Contains(input) && !AppConstants.DeclineCommands.Contains(input)))
            {
                Console.WriteLine(message);
                Console.Write("> ");
                input = Console.ReadLine();
            }

            return AppConstants.ConfirmationCommands.Contains(input);
        }
    }
}