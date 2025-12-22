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
        public IEnumerable<(string, bool, string)> SortMediaFilesByDate(string writePath, IDictionary<string, DateMetadata> mediaWithMetadata)
        {
            var unknownDateWritePath = Path.Join(writePath, "unknown");
            FileUtils.CreateDirectoryIfDoesntExist(unknownDateWritePath);

            // TODO - multithread this
            foreach (var media in mediaWithMetadata)
            {
                (bool, string) copyStatus = new();
                if (media.Value.DateTaken == DateTime.MinValue)
                {
                    copyStatus = SaveMediaWithUnknownDate(unknownDateWritePath, media.Key);
                }
                else
                {
                    copyStatus = SaveMediaWithDate(writePath, media.Key, media.Value.DateTaken);
                }

                _logger.LogDebug("\"{mediaFile}\" sorting status: {success}. Message: {message}", media.Key, copyStatus.Item1 ? "Successful" : "Unsuccesful", copyStatus.Item2);

                yield return new(media.Key, copyStatus.Item1, copyStatus.Item2);
            }
        }

        private (bool, string) SaveMediaWithDate(string baseWritePath, string mediaFile, DateTime dateTaken)
        {
            var newFileName = dateTaken.Date.ToString("yyyyMMdd") + "_" + Path.GetFileName(mediaFile);
            var yearTaken = dateTaken.Date.ToString("yyyy");
            var monthTaken = dateTaken.Date.ToString("MM MMMM");

            var outputDirectory = Path.Join(baseWritePath, yearTaken, monthTaken);
            FileUtils.CreateDirectoryIfDoesntExist(outputDirectory);

            var destinationFilePath = Path.Join(outputDirectory, newFileName);
            _logger.LogDebug("Attempting to save \"{file}\" to \"{destination}\"", mediaFile, destinationFilePath);

            return FileUtils.CopyFile(mediaFile, destinationFilePath);
        }

        private (bool, string) SaveMediaWithUnknownDate(string baseWriteFilePath, string mediaFile)
        {
            _logger.LogDebug("Couldn't determine date taken for {file}. Saving to \"unkown\" folder", mediaFile);
            var destinationFile = Path.GetFileName(mediaFile);
            var destinationFilePath = Path.Join(baseWriteFilePath, destinationFile);

            return FileUtils.CopyFile(mediaFile, destinationFilePath);
        }
    }
}