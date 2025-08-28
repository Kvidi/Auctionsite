using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public enum SellerType
    {
        All,
        PrivateOnly,
        CompanyOnly
    }

    // New enum for search types
    public enum SearchType
    {
        Contains,      // Default: search anywhere in title/description
        ExactPhrase,   // Search for exact phrase (consecutive characters)
        StartsWith     // Search for terms that start with the search phrase
    }
    public class AdSearchFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public SearchType SearchType { get; set; } = SearchType.ExactPhrase; // Default to exact phrase
        public int? CategoryId { get; set; }
        public List<string>? SelectedLocations { get; set; } = new();
        public List<int>? SelectedCategoryIds { get; set; } = new();
        public List<Condition>? SelectedConditions { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public AdType? AdType { get; set; }
        public SellerType SellerType { get; set; } = SellerType.All;
    }

}
