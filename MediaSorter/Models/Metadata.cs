namespace MediaSorter.Models
{
    public record RawMetadata(string Directory, string Name, string Description);

    public record DateMetadata(string Directory, string Name, string Description, DateTime DateTaken, double AccuracyWeight);
}
