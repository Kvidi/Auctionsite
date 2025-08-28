namespace Auctionsite.Helpers
{
    public static class CategoryIconHelper
    {
        public static string GetCategoryIcon(string name)
        {
            if (name.Contains("Sport")) return "bicycle";
            if (name.Contains("Biljetter")) return "ticket";
            if (name.Contains("Telefoni")) return "mobile";
            if (name.Contains("Övrigt")) return "ellipsis-h";
            return "folder";
        }
    }
}
