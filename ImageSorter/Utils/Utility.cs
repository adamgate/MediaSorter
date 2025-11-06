namespace MediaSorter.Utils
{
    public static class Utility
    {
        public static bool EqualsIgnoreCase(this string val1, string val2) =>
            string.Equals(val1, val2, StringComparison.OrdinalIgnoreCase);
    }
}
