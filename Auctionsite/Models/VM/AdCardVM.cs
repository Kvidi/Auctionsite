using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class AdCardVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HeadImageUrl { get; set; } = string.Empty;
        public DateTime? AuctionEndDate { get; set; }
        public bool IsEnded => AuctionEndDate.HasValue && AuctionEndDate.Value <= DateTime.Now;
        public DateTime? ApprovedAt { get; set; }       
        public bool IsApproved => ApprovedAt.HasValue; // Indicates if the ad has been approved
        public bool IsRejected { get; set; }
        public AdType AdType { get; set; }
        public decimal? StartingPrice { get; set; }
        public decimal? BuyNowPrice { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public int BidCount { get; set; }

        public bool IsFavourite { get; set; }
    }
}
