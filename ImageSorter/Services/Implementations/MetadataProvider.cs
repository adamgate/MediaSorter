using ImageSorter.Services.Interfaces;
using ImageSorter.Utils;
using MetadataExtractor;

namespace ImageSorter.Services.Implementations
{
    public class MetadataProvider : IMetadataProvider
    {
        private record RawMetadata(string Directory, string Name, string Description);

        /// <summary>
        /// Extacts the date taken metadata from the provided images.
        /// </summary>
        /// <param name="images">The image paths.</param>
        /// <returns></returns>
        public IDictionary<string, DateTime?> GetDateTakenMetadata(IEnumerable<string> images)
        {
            var assortedDateMetadata = GetRawDateMetadata(images);

            var parsedMetadata = new Dictionary<string, DateTime?>();
            foreach (var image in images)
            {
                var dateTaken = DetermineMostAccurateDate(assortedDateMetadata[image]);
                parsedMetadata.Add(image, dateTaken);
            }

            return parsedMetadata;
        }

        private DateTime? DetermineMostAccurateDate(List<RawMetadata> imageMetadata)
        {
            if (!imageMetadata.Any())
                return null;

            // This still doesn't work the way I intend. It will grab the first of ANY of these values that matches
            // TODO - Figure out how to prioritize certain fields without doing repeated loops
            foreach (var tag in imageMetadata)
            {
                if (tag.Directory.Contains("Exif") && tag.Name.EqualsIgnoreCase("Date/Time Original"))
                    return DateTime.Parse(tag.Description);

                if (tag.Name.EqualsIgnoreCase("GPS Date Stamp"))
                    return DateTime.Parse(tag.Description);

                if (tag.Name.EqualsIgnoreCase("Date/Time Digitized"))
                    return DateTime.Parse(tag.Description);

                if (tag.Name.EqualsIgnoreCase("Date/Time"))
                    return DateTime.Parse(tag.Description);
            }

            return null;
        }

        private IDictionary<string, List<RawMetadata>> GetRawDateMetadata(IEnumerable<string> images)
        {
            var aggregatedImageMetdata = new Dictionary<string, List<RawMetadata>>();

            foreach (var image in images)
            {
                var rawMetadata = ImageMetadataReader.ReadMetadata(image);

                var imageDateMetadata = new List<RawMetadata>();
                foreach (var directory in rawMetadata)
                {
                    var directoryDateMetadata = directory.Tags
                        .Where(x => x.Name.Contains("Date"))
                        .Select(x => new RawMetadata(x.DirectoryName, x.Name, x.Description ?? ""));

                    imageDateMetadata.AddRange(directoryDateMetadata);
                }

                aggregatedImageMetdata.Add(image, imageDateMetadata);
            }

            return aggregatedImageMetdata;
        }
    }
}