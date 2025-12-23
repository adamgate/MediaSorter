using MediaSorter.Models;

namespace MediaSorter.Services.Interfaces
{
    public interface IFileSorter
    {
        IEnumerable<(string, bool, string)> SortMediaFilesByDate(string path, IDictionary<string, DateMetadata> mediaWithMetadata);
    }
}