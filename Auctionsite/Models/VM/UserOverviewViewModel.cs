using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class UserOverviewViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public PagedResult<AdCardVM> Annonser { get; set; } = new();
        public List<Review> Omdomen { get; set; } = new();
    }
}
