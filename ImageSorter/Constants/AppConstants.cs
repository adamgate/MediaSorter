namespace MediaSorter.Constants
{
    public static class AppConstants
    {
        public static readonly string[] SupportedMediaFormats =
        [
            // Images
            ".png",
            ".jpg",
            ".jpeg",
            ".heic",
            ".heif",
            ".avif",
            ".bmp",
            ".dng",
            ".gif",
            ".ico",
            ".jfif",
            ".webp",
            // Video
            ".avci",
            ".avi",
            ".mov",
            ".mp4"
        ];

        public static readonly char[] IllegalFileCharacters = ['/', '\\', '.', '<', '>', ':', '"', '|', '?', '*'];

        public static readonly string[] TerminationCommands = ["Exit", "exit", "X", "x"];

        public static readonly string[] ConfirmationCommands = ["Yes", "yes", "Y", "y"];

        public static readonly string[] DeclineCommands = ["No", "no", "N", "n"];
    }
}