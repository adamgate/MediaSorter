namespace ImageSorter.Services.Interfaces
{
    public interface IFileProcessor
    {
        IEnumerable<string> GetImagesInPath(string path);
    }
}