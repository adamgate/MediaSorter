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
        /// Extracts the date taken metadata from the provided media.
        /// </summary>
        public IDictionary<string, IEnumerable<RawMetadata>> EvaluateMediaMetadata(IEnumerable<string> mediaPaths)
        {
            var mediaWithMetadata = new Dictionary<string, IEnumerable<RawMetadata>>();

            foreach (var mediaItem in mediaPaths)
            {
                IReadOnlyList<MetadataExtractor.Directory>? rawMetadata = null;
                try
                {
                    rawMetadata = ImageMetadataReader.ReadMetadata(mediaItem);
                    _logger.LogDebug("Raw metadata for \"{media}\" : {rawMetadata}", mediaItem, @rawMetadata);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Unable to get metadata for file \"{fileName}\". Error in MetadataExtractor: {exception}", mediaItem, @ex);
                }
                finally
                {
                    var mediaItemDateMetadata = new List<RawMetadata>();

                    if (rawMetadata is null)
                    {
                        _logger.LogDebug("No metadata found for file \"{fileName}\"", mediaItem);
                    }
                    else
                    {
                        foreach (var directory in rawMetadata)
                        {
                            var directoryDateMetadata = directory.Tags
                                .Where(x => !x.Name.EqualsIgnoreCase("File Modified Date")
                                    && (x.Name.Contains("Date") || x.Name.Contains("Created")))
                                .Select(x => new RawMetadata(x.DirectoryName, x.Name, x.Description ?? ""));

                            mediaItemDateMetadata.AddRange(directoryDateMetadata);
                        }
                    }

                    mediaWithMetadata.Add(mediaItem, mediaItemDateMetadata);
                }
            }

            return mediaWithMetadata;
        }
    }
}