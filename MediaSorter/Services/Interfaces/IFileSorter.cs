using MediaSorter.Models;

namespace MediaSorter.Services.Interfaces
{
    public interface IFileSorter
    {
        void SortMediaFilesByDate(string path, IDictionary<string, DateMetadata> mediaWithMetadata);
    }
}