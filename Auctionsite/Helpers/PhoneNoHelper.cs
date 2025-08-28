using System.Text.RegularExpressions;

namespace Auctionsite.Helpers
{
    public class PhoneNoHelper
    {
        public static string FormatNr(string nr)
        {

            if (string.IsNullOrWhiteSpace(nr))
            {
                return "";
            }

            nr = Regex.Replace(nr, @"[^\d]", "");

            if (nr.StartsWith("07") && nr.Length == 10)
            {
                return nr;
            }
            else if (nr.StartsWith("467") && nr.Length == 11)
            {
                return string.Concat("0", nr.AsSpan(2));
            }
            else if (nr.StartsWith("00467") && nr.Length == 13)
            {
                return string.Concat("0", nr.AsSpan(4));
            }
            else if (nr.StartsWith("7") && nr.Length == 9)
            {
                return "0" + nr;
            }
            return "";
        }

    }
}
