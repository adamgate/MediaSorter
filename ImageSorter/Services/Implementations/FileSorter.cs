using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Sorts the provided media files.
    /// </summary>
    public class FileSorter : IFileSorter
    {
        /// <summary>
        ///  Attempts to sort media files by their taken date if possible, otherwise by their original name
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mediaWithMetadata"></param>
        public void SortMediaFilesByDate(string path, IDictionary<string, string> mediaWithMetadata)
        {
            var unknownDirectory = Path.Join(path, "unknown");
            FileUtils.CreateDirectoryIfDoesntExist(unknownDirectory);

            foreach (var media in mediaWithMetadata)
            {
                var isParseDateTakenSuccessful = DateTime.TryParse(media.Value, out var dateTaken);

                if (!isParseDateTakenSuccessful)
                    SaveMediaWithUnknownDate(path, media.Key, media.Value);
                else
                    SaveMediaWithDate(path, media.Key, dateTaken);
            }
        }

        private void SaveMediaWithDate(string baseWritePath, string mediaFile, DateTime dateTaken)
        {
            var newFileName = dateTaken.Date.ToString("yyyy-MM-dd") + Path.GetExtension(mediaFile);

            var yearTaken = dateTaken.Date.ToString("yyyy");
            var monthTaken = dateTaken.Date.ToString("MM");
            var dayTaken = dateTaken.Date.ToString("dd");

            var yearDirectory = Path.Join(baseWritePath, yearTaken);
            var monthDirectory = Path.Join(yearDirectory, monthTaken);
            var dayDirectory = Path.Join(monthDirectory, dayTaken);

            FileUtils.CreateDirectoryIfDoesntExist(yearDirectory);
            FileUtils.CreateDirectoryIfDoesntExist(monthDirectory);
            FileUtils.CreateDirectoryIfDoesntExist(dayDirectory);

            var destFilePath = Path.Join(baseWritePath, newFileName);

            FileUtils.CopyFile(baseWritePath, destFilePath);
        }

        private void SaveMediaWithUnknownDate(
            string baseWriteFilePath,
            string sourceMediaFile,
            string newFileName
        )
        {
            var processedName =
                FileUtils.StripIllegalFileCharacters(newFileName)
                ?? Path.GetFileName(sourceMediaFile);
            var destFilePath = Path.Join(baseWriteFilePath, processedName);

            FileUtils.CopyFile(sourceMediaFile, destFilePath);
        }
    }
}
