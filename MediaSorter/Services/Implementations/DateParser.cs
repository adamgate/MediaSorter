using MediaSorter.Constants;
using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Parses date metadata into DateTime.
    /// </summary>
    public class DateParser : IDateParser
    {
        private readonly ILogger<DateParser> _logger;

        public DateParser(ILogger<DateParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Attempts to parse the raw metadata date strings into <see cref="DateTime"/> and selects the most accurate date for each media path.
        /// </summary>
        /// <param name="mediaPathsWithMetadata"></param>
        /// <returns></returns>
        public IDictionary<string, DateMetadata> Parse(IDictionary<string, IEnumerable<RawMetadata>> mediaPathsWithMetadata)
        {
            var mediaWithDateMetadata = new Dictionary<string, DateMetadata>();

            foreach (var media in mediaPathsWithMetadata)
            {
                if (!media.Value.Any())
                {
                    mediaWithDateMetadata.Add(media.Key, new DateMetadata("", "", "", DateTime.MinValue, 0));
                    _logger.LogDebug("No date metadata found for \"{mediaPath}\"", media.Key);
                    continue;
                }

                var parsedDates = media.Value.Select(x => Parse(x)).ToList();
                _logger.LogDebug("Media Path: \"{mediaPath}\" | {parsedDates}", media.Key, string.Join(",", parsedDates));

                var mostAccurateDate = SelectMostAccurateDate(parsedDates);
                _logger.LogDebug("Most accurate date selected for {mediaPath}: {mostAccurateDate}", media.Key, mostAccurateDate);

                mediaWithDateMetadata.Add(media.Key, mostAccurateDate);
            }

            return mediaWithDateMetadata;
        }

        private static DateMetadata Parse(RawMetadata rawMetadata)
        {
            var dateFormat = (rawMetadata.Directory) switch
            {
                var directory when directory.Contains(MetadataConstants.EXIF, StringComparison.OrdinalIgnoreCase) => MetadataConstants.ExifDateFormat,
                var directory when directory.Contains(MetadataConstants.IPTC, StringComparison.OrdinalIgnoreCase) => MetadataConstants.IptcDateFormat,
                var directory when directory.Contains(MetadataConstants.GPS, StringComparison.OrdinalIgnoreCase) => MetadataConstants.GpsDateFormat,
                var directory when directory.Contains(MetadataConstants.File, StringComparison.OrdinalIgnoreCase) => MetadataConstants.FileDateFormat,
                var directory when directory.Contains(MetadataConstants.ICC, StringComparison.OrdinalIgnoreCase) => MetadataConstants.IccDateFormat,
                var directory when directory.Contains(MetadataConstants.QuickTime, StringComparison.OrdinalIgnoreCase) => MetadataConstants.QuickTimeDateFormat,
                _ => "default" // Use default DateTime parsing
            };

            var accuracyWeight = WeightDates(rawMetadata.Name);
            var dateTaken = DateTime.MinValue;

            if (dateFormat.Equals("default"))
                DateTime.TryParse(rawMetadata.Description, out dateTaken);
            else
                DateTime.TryParseExact(rawMetadata.Description, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTaken);

            return new DateMetadata(rawMetadata.Directory, rawMetadata.Name, rawMetadata.Description, dateTaken, accuracyWeight);
        }

        private static DateMetadata SelectMostAccurateDate(IEnumerable<DateMetadata> dateMetadata)
            => dateMetadata.OrderByDescending(x => x.AccuracyWeight).First();

        private static double WeightDates(string Name)
            => (Name.ToLower()) switch
            {
                ("date/time original") => 0.9, // EXIF, IPTC
                ("created") => 0.9, // for QuickTime videos
                ("gps date stamp") => 0.8,
                ("date/time digitized") => 0.7,
                ("date/time") => 0.6,
                ("file modified date") => 0.0, // This is useless for evaluating the date taken.
                _ => 0.1
            };
    }
}