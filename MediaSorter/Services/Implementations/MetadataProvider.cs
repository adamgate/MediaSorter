using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MetadataExtractor;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Extracts the date metadata from provided media files.
    /// </summary>
    public class MetadataProvider : IMetadataProvider
    {
        /// <summary>
        /// Extacts the date taken metadata from the provided media.
        /// </summary>
        /// <param name="mediaPaths">The media paths.</param>
        /// <returns></returns>
        public IDictionary<string, IEnumerable<RawMetadata>> EvaluateMediaMetadata(IEnumerable<string> mediaPaths)
        {
            var mediaWithMetadata = new Dictionary<string, IEnumerable<RawMetadata>>();

            foreach (var mediaItem in mediaPaths)
            {
                var rawMetadata = ImageMetadataReader.ReadMetadata(mediaItem);

                var mediaItemDateMetadata = new List<RawMetadata>();
                foreach (var directory in rawMetadata)
                {
                    var directoryDateMetadata = directory.Tags
                        .Where(x => x.Name.Contains("Date"))
                        .Select(x => new RawMetadata(x.DirectoryName, x.Name, x.Description ?? ""));

                    mediaItemDateMetadata.AddRange(directoryDateMetadata);
                }

                mediaWithMetadata.Add(mediaItem, mediaItemDateMetadata);
            }

            return mediaWithMetadata;
        }
    }
}
