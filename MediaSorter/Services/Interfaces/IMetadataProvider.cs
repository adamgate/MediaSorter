namespace MediaSorter.Services.Interfaces
{
    public interface IMetadataProvider
    {
        IDictionary<string, string> EvaluateMediaMetadata(IEnumerable<string> media);
    }
}