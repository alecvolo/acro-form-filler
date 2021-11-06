namespace AcroFormFiller
{
    public static class StringHelper
    {
        public static string TrimToNull(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        public static string AppendIfNotEmpty(this string value, string appendValue)
        {
            return string.IsNullOrWhiteSpace(value) ? null : string.Concat(value.Trim(), appendValue);
        }
    }

}
