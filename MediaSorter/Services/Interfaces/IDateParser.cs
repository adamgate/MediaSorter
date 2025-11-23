using MediaSorter.Models;

namespace MediaSorter.Services.Interfaces
{
    public interface IDateParser
    {
        IDictionary<string, DateMetadata> Parse(IDictionary<string, IEnumerable<RawMetadata>> mediaPathsWithMetadata);
    }
}
