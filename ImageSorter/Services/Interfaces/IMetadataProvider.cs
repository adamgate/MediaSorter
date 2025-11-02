namespace ImageSorter.Services.Interfaces
{
    public interface IMetadataProvider
    {
        IDictionary<string, DateTime?> GetDateTakenMetadata(IEnumerable<string> images);
    }
}