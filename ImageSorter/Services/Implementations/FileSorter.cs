using MediaSorter.Services.Interfaces;

namespace MediaSorter.Services.Implementations
{
    public class FileSorter : IFileSorter
    {
        public void SortMediaFilesByDate(string path, IDictionary<string, string> mediaWithMetadata)
        {
            // loop through every image and do the following:
            //
            //  - attempt to parse the date to a DateTime
            //  - if successful:
            //      - copy each file to its own folder based on the date
            //      - create folders if they don't exist
            //      - name the media file in YYYY-MM-DD format
            //      - handle name collisions
            //
            //  - if parsing is unsuccessful:
            //      - copy file to an "unknown date" folder
            //      - name the media file with the "date", stripping out non-filesafe characters
            //      - if the string is empty, name it the original name of the file
        }
    }
}
