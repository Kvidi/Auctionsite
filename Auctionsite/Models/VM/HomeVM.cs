using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class HomeVM
    {
        public List<AdCardVM> LastChanceAds { get; set; } = [];
        public List<AdCardVM> NewAds { get; set; } = [];
        public List<AdCardVM> PopularAds { get; set; } = [];
        public AdSearchFilterViewModel SearchFilter { get; set; } = new AdSearchFilterViewModel();

    }
}
