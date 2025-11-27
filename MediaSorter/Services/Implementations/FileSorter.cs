using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using Microsoft.Extensions.Logging;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Sorts the provided media files.
    /// </summary>
    public class FileSorter : IFileSorter
    {
        private readonly ILogger<FileSorter> _logger;

        public FileSorter(ILogger<FileSorter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///  Attempts to sort media files by their taken date if possible, otherwise by their original name.
        /// </summary>
        /// <param name="writePath"></param>
        /// <param name="mediaWithMetadata"></param>
        public void SortMediaFilesByDate(string writePath, IDictionary<string, DateMetadata> mediaWithMetadata)
        {
            var unknownDateWritePath = Path.Join(writePath, "unknown");
            FileUtils.CreateDirectoryIfDoesntExist(unknownDateWritePath);

            foreach (var media in mediaWithMetadata)
            {
                if (media.Value.DateTaken == DateTime.MinValue)
                    SaveMediaWithUnknownDate(unknownDateWritePath, media.Key, media.Value.Description);
                else
                    SaveMediaWithDate(writePath, media.Key, media.Value.DateTaken);

                CliUtils.DisplayMessageWithColor($"Sorted {Path.GetFileName(media.Key)}", ConsoleColor.Gray);
            }
        }

        private void SaveMediaWithDate(string baseWritePath, string mediaFile, DateTime dateTaken)
        {
            var newFileName = dateTaken.Date.ToString("yyyyMMdd") + "_" + Path.GetFileName(mediaFile);
            var yearTaken = dateTaken.Date.ToString("yyyy");
            var monthTaken = dateTaken.Date.ToString("MM MMMM");

            var outputDirectory = Path.Join(baseWritePath, yearTaken, monthTaken);
            FileUtils.CreateDirectoryIfDoesntExist(outputDirectory);

            var destinationFilePath = Path.Join(outputDirectory, newFileName);
            _logger.LogDebug("Attempting to save \"{file}\" to \"{destination}\"", mediaFile, destinationFilePath);
            FileUtils.CopyFile(mediaFile, destinationFilePath);
        }

        private void SaveMediaWithUnknownDate(string baseWriteFilePath, string mediaFile, string newFileName)
        {
            _logger.LogDebug("Couldn't determine date taken for {file}. Saving to \"unkown\" folder", mediaFile);
            var destinationFile = Path.GetFileName(mediaFile);
            var destinationFilePath = Path.Join(baseWriteFilePath, destinationFile);

            FileUtils.CopyFile(mediaFile, destinationFilePath);
        }
    }
}