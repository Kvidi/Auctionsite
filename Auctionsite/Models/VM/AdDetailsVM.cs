using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class AdDetailsVM
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public decimal? StartingPrice { get; set; }
        public decimal? MinimumEndPrice { get; set; }
        public decimal? BuyNowPrice { get; set; }

        public decimal CurrentHighestBid { get; set; } 
        public decimal LeadingMaxbid { get; set; }
        public decimal MinimumBid { get; set; } 
        public int BidCount { get; set; }
        public bool IsLeadingBidder { get; set; } 
        public bool IsOutbid { get; set; }
        public string? LeadingBidderUserName { get; set; }

        public string? HeadImageURL { get; set; }
        public string? VideoURL { get; set; }
        public List<AdvertisementImage> Images { get; set; }  = new();

        public DateTime AddedAt { get; set; }
        public DateTime? AuctionEndDate { get; set; }
        public bool IsEnded => AuctionEndDate.HasValue && AuctionEndDate.Value <= DateTime.Now;

        public bool IsCompanySeller { get; set; }
        public bool AvailableForPickup { get; set; }
        public string? PickupLocation { get; set; }

        public Condition? Condition { get; set; }
        public AdType? AdType { get; set; }

        public bool WasBought { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public string? PurchasedByUserName { get; set; }

        public string? UserName { get; set; }
        public bool IsFavourite { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsRejected { get; set; }

        public List<CategoryForAdvertisement> Breadcrumbs { get; set; } = new();
        public List<CategoryForAdvertisement> SubCategories { get; set; } = new();

        public string? ShippingMethod { get; set; }
        public decimal? ShippingCost { get; set; }        
        public List<string> PaymentMethods { get; set; }  = new();
    }
}
