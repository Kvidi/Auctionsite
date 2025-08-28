namespace Auctionsite.Helpers
{
    // This class provides extension methods for string manipulation.
    public static class StringExtensions
    {
        // Truncates a string to a specified maximum length and appends "..." if it exceeds that length.
        public static string Truncate(this string value, int maxLength) 
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
