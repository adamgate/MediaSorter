using MediaSorter.Models;
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

                // TODO - log this so the class is decoupled from CLI 
                CliUtils.DisplayMessageWithColor($"Sorted {Path.GetFileName(media.Key)}", ConsoleColor.Gray);
            }
        }

        private static void SaveMediaWithDate(string baseWritePath, string mediaFile, DateTime dateTaken)
        {
            var newFileName = dateTaken.Date.ToString("yyyyMMdd") + "_" + Path.GetFileName(mediaFile);
            var yearTaken = dateTaken.Date.ToString("yyyy");
            var monthTaken = dateTaken.Date.ToString("MM MMMM");

            var outputDirectory = Path.Join(baseWritePath, yearTaken, monthTaken);
            FileUtils.CreateDirectoryIfDoesntExist(outputDirectory);

            var destinationFilePath = Path.Join(outputDirectory, newFileName);
            FileUtils.CopyFile(mediaFile, destinationFilePath);
        }

        private static void SaveMediaWithUnknownDate(string baseWriteFilePath, string mediaFile, string newFileName)
        {
            var destinationFile = Path.GetFileName(mediaFile);
            var destinationFilePath = Path.Join(baseWriteFilePath, destinationFile);

            FileUtils.CopyFile(mediaFile, destinationFilePath);
        }
    }
}