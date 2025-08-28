namespace Auctionsite.Models.VM
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Auctionsite.Models.Database;

    // View model used when browsing and filtering ads
    public class BrowseAdsViewModel
    {
        // The user's selected filter options
        public AdSearchFilterViewModel FilterVM { get; set; } = new();

        // Available options for the filter (for filling the dropdowns and checkboxes)
        public List<SelectListItem> AvailableCategories { get; set; } = new();
        public List<SelectListItem> ConditionOptions { get; set; } = new();
        public List<SelectListItem> AdTypeOptions { get; set; } = new();

        // The list of advertisements that match the filter
        public PagedResult<Advertisement> Results { get; set; } = new();
        public List<AdCardVM> AdCards { get; set; } = new();
    }

}
