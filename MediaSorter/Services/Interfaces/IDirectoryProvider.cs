namespace MediaSorter.Services.Interfaces
{
    public interface IDirectoryProvider
    {
        string? GetValidDirectory(string message);
    }
}