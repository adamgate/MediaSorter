namespace MediaSorter.Utils
{
    public static class StringUtils
    {
        public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (string element in source)
            {
                if (value.EqualsIgnoreCase(element))
                    return true;
            }

            return false;
        }

        public static bool EqualsIgnoreCase(this string val1, string val2)
                    => string.Equals(val1, val2, StringComparison.OrdinalIgnoreCase);
    }
}