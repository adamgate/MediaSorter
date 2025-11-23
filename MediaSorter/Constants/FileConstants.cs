namespace MediaSorter.Constants
{
    public static class FileConstants
    {
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
    }
}