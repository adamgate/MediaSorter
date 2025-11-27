using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace MediaSorter.Services.Implementations
{
    /// <summary>
    /// Extracts metadata from provided files.
    /// </summary>
    public class MetadataProvider : IMetadataProvider
    {
        private readonly ILogger<MetadataProvider> _logger;

        public MetadataProvider(ILogger<MetadataProvider> logger)
        {
            _logger = logger;
        }

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
                _logger.LogDebug("Raw metadata for \"{media}\" : {rawMetadata}", mediaItem, @rawMetadata);

                var mediaItemDateMetadata = new List<RawMetadata>();
                foreach (var directory in rawMetadata)
                {
                    var directoryDateMetadata = directory.Tags
                        .Where(x => !x.Name.EqualsIgnoreCase("File Modified Date") && (x.Name.Contains("Date") || x.Name.Contains("Created")))
                        .Select(x => new RawMetadata(x.DirectoryName, x.Name, x.Description ?? ""));

                    mediaItemDateMetadata.AddRange(directoryDateMetadata);
                }

                mediaWithMetadata.Add(mediaItem, mediaItemDateMetadata);
            }

            return mediaWithMetadata;
        }
    }
}