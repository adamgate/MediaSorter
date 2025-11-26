namespace MediaSorter.Services.Interfaces
{
    public interface IMediaScanner
    {
        IEnumerable<string> GetMediaInPath(string path);
    }
}