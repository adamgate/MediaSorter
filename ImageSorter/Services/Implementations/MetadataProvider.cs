using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using MetadataExtractor;

namespace MediaSorter.Services.Implementations
{
    public class MetadataProvider : IMetadataProvider
    {
        private record RawMetadata(string Directory, string Name, string Description);

        private record WeightedMetadata(
            string Directory,
            string Name,
            string Description,
            double AccuracyWeight
        );

        /// <summary>
        /// Extacts the date taken metadata from the provided media.
        /// </summary>
        /// <param name="mediaPaths">The media paths.</param>
        /// <returns></returns>
        public IDictionary<string, string> EvaluateMediaMetadata(IEnumerable<string> mediaPaths)
        {
            var assortedDateMetadata = GetRawDateMetadata(mediaPaths);

            var parsedMetadata = new Dictionary<string, string>();
            foreach (var media in assortedDateMetadata)
            {
                var dateTaken = media.Value
                    .Select(
                        x =>
                            new WeightedMetadata(x.Directory, x.Name, x.Description, WeightDates(x))
                    )
                    .OrderByDescending(x => x.AccuracyWeight)
                    .First();

                parsedMetadata.Add(media.Key, dateTaken.Description);
            }

            return parsedMetadata;
        }

        // TODO - provide different weights based on desired dates, like if the user wants to prioritize the date digitized.
        /// <summary>
        /// Weights the date metadata on how accurate it is likely to be, preferring EXIF metadata.
        /// </summary>
        private double WeightDates(RawMetadata rawMetadata)
        {
            if (
                rawMetadata.Directory.Contains("Exif")
                && rawMetadata.Name.EqualsIgnoreCase("Date/Time Original")
            )
                return 0.9;

            if (rawMetadata.Name.EqualsIgnoreCase("GPS Date Stamp"))
                return 0.8;

            if (rawMetadata.Name.EqualsIgnoreCase("Date/Time Digitized"))
                return 0.7;

            if (rawMetadata.Name.EqualsIgnoreCase("Date/Time"))
                return 0.6;

            if (rawMetadata.Name.EqualsIgnoreCase("File Modified Date"))
                return 0.0;

            return 0.1;
        }

        private IDictionary<string, List<RawMetadata>> GetRawDateMetadata(
            IEnumerable<string> mediaPaths
        )
        {
            var mediaWithMetadata = new Dictionary<string, List<RawMetadata>>();

            foreach (var media in mediaPaths)
            {
                var rawMetadata = ImageMetadataReader.ReadMetadata(media);

                var mediaDateMetadata = new List<RawMetadata>();
                foreach (var directory in rawMetadata)
                {
                    var directoryDateMetadata = directory.Tags
                        .Where(x => x.Name.Contains("Date"))
                        .Select(x => new RawMetadata(x.DirectoryName, x.Name, x.Description ?? ""));

                    mediaDateMetadata.AddRange(directoryDateMetadata);
                }

                mediaWithMetadata.Add(media, mediaDateMetadata);
            }

            return mediaWithMetadata;
        }
    }
}
