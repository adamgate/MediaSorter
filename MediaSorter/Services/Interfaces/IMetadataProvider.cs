using MediaSorter.Models;

namespace MediaSorter.Services.Interfaces
{
    public interface IMetadataProvider
    {
        IDictionary<string, IEnumerable<RawMetadata>> EvaluateMediaMetadata(IEnumerable<string> media);
    }
}