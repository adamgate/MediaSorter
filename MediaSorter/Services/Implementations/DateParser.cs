using MediaSorter.Constants;
using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using System.Globalization;

namespace MediaSorter.Services.Implementations
{
    public class DateParser : IDateParser
    {
        private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";
        private const string FileDateFormat = "ddd MMM dd HH:mm:ss zzz yyyy";
        private const string GpsDateFormat = "yyyy:MM:dd";
        private const string IccDateFormat = "yyyy:MM:dd HH:mm:ss";
        private const string IptcDateFormat = "MM/dd/yyyy";
        private const string QuickTimeDateFormat = "ddd MMM dd HH:mm:ss yyyy";

        public IDictionary<string, DateMetadata> Parse(IDictionary<string, IEnumerable<RawMetadata>> mediaPathsWithMetadata)
        {
            var mediaWithDateMetadata = new Dictionary<string, DateMetadata>();

            foreach (var media in mediaPathsWithMetadata)
            {
                if (!media.Value.Any())
                {
                    mediaWithDateMetadata.Add(media.Key, new DateMetadata("", "", "", DateTime.MinValue, 0));
                    continue;
                }

                var parsedDates = media.Value.Select(x => Parse(x)).ToList();
                var mostAccurateDate = SelectMostAccurateDate(parsedDates);

                mediaWithDateMetadata.Add(media.Key, mostAccurateDate);
            }

            return mediaWithDateMetadata;
        }

        private static DateMetadata Parse(RawMetadata rawMetadata)
        {
            var dateFormat = (rawMetadata.Directory) switch
            {
                var directory when directory.Contains(MetadataConstants.EXIF, StringComparison.OrdinalIgnoreCase) => ExifDateFormat,
                var directory when directory.Contains(MetadataConstants.IPTC, StringComparison.OrdinalIgnoreCase) => IptcDateFormat,
                var directory when directory.Contains(MetadataConstants.GPS, StringComparison.OrdinalIgnoreCase) => GpsDateFormat,
                var directory when directory.Contains(MetadataConstants.File, StringComparison.OrdinalIgnoreCase) => FileDateFormat,
                var directory when directory.Contains(MetadataConstants.ICC, StringComparison.OrdinalIgnoreCase) => IccDateFormat,
                var directory when directory.Contains(MetadataConstants.QuickTime, StringComparison.OrdinalIgnoreCase) => QuickTimeDateFormat,
                _ => "default"
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