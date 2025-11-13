namespace MediaSorter.Constants
{
    public static class AppConstants
    {
        public static readonly string[] ConfirmationCommands = ["Yes", "Y"];

        public static readonly string[] DeclineCommands = ["No", "N"];

        public static readonly char[] IllegalFileCharacters = ['/', '\\', '.', '<', '>', ':', '"', '|', '?', '*'];

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

        public static readonly string[] TerminationCommands = ["Exit", "X"];
    }
}