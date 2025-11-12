using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using System.Text;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Sorts the provided media files.
    /// </summary>
    public class FileSorter : IFileSorter
    {
        /// <summary>
        ///  Attempts to sort media files by their taken date if possible, otherwise by their original name.
        /// </summary>
        /// <param name="writePath"></param>
        /// <param name="mediaWithMetadata"></param>
        public void SortMediaFilesByDate(string writePath, IDictionary<string, string> mediaWithMetadata)
        {
            var unknownDateWritePath = Path.Join(writePath, "unknown");
            FileUtils.CreateDirectoryIfDoesntExist(unknownDateWritePath);

            foreach (var media in mediaWithMetadata)
            {
                var isParseDateTakenSuccessful = DateTime.TryParse(media.Value, out var dateTaken);

                if (!isParseDateTakenSuccessful)
                    SaveMediaWithUnknownDate(unknownDateWritePath, media.Key, media.Value);
                else
                    SaveMediaWithDate(writePath, media.Key, dateTaken);
            }
        }

        private void SaveMediaWithDate(string baseWritePath, string mediaFile, DateTime dateTaken)
        {
            var newFileName = dateTaken.Date.ToString("yyyy-MM-dd") + Path.GetExtension(mediaFile);
            var yearTaken = dateTaken.Date.ToString("yyyy");
            var monthTaken = dateTaken.Date.ToString("MMMM");

            var outputDirectory = Path.Join(baseWritePath, yearTaken, monthTaken);
            FileUtils.CreateDirectoryIfDoesntExist(outputDirectory);

            var destFilePath = Path.Join(outputDirectory, newFileName);
            FileUtils.CopyFile(mediaFile, destFilePath);
        }

        private void SaveMediaWithUnknownDate(string baseWriteFilePath, string mediaFile, string newFileName)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(Path.GetFileNameWithoutExtension(mediaFile));
            stringBuilder.Append(" -- ");
            stringBuilder.Append(FileUtils.StripIllegalFileCharacters(newFileName));
            stringBuilder.Append(Path.GetExtension(mediaFile));

            var destFile = stringBuilder.ToString();
            var destFilePath = Path.Join(baseWriteFilePath, destFile);

            FileUtils.CopyFile(mediaFile, destFilePath);
        }
    }
}