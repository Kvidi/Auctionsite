using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctionsite.Models.Database
{

    public class Advertisement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Description { get; set; }

        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal? StartingPrice { get; set; }

        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal? MinimumEndPrice { get; set; }

        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal? BuyNowPrice { get; set; }
                
        [MaxLength(2048)]
        public string? VideoURL { get; set; }

        public virtual ICollection<AdvertisementImage> Images { get; set; } = new List<AdvertisementImage>();

        [Required]
        public DateTime AddedAt { get; set; }
        public bool IsSeenByAdmin { get; set; }

        // Properties for rejection
        public bool IsRejected { get; set; } = false;
        public string? RejectionReason { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime? AuctionEndDate { get; set; } // The date when the auction ends.

        public bool IsCompanySeller { get; set; }

        public bool AvailableForPickup { get; set; } // If the ad is available for pickup or not
                
        [MaxLength(100)]
        public string? PickupLocation { get; set; }

        public Condition Condition { get; set; }

        public AdType AdType { get; set; }
        public int ViewCount { get; set; } = 0; // The number of times the ad has been viewed
        public virtual User Advertiser { get; set; }
        public virtual ICollection<User> UsersWhoFavourited { get; set; } = new List<User>(); // Users who favourited this ad

        public int CategoryId { get; set; }
        public CategoryForAdvertisement Category { get; set; } = null!;
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public decimal CurrentHighestBid { get; set; } 

        public ICollection<MaxBid> MaxBids { get; set; } = new List<MaxBid>(); // The max bids for this ad
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();// The automatically incremented, publically displayed, bids for this ad

        public DateTime? PurchasedAt { get; set; } // When the ad was sold
        public string? PurchasedByUserId { get; set; } // Id of whom it was sold to                
        public virtual User? PurchasedBy { get; set; } // Navigation property for the user who purchased the ad


        // ShippingMethod

        // ShippingCost

        // ShippingTime

        // PaymentMethods
    }

    // The enums need to be outside the class in order for the querying to work
    public enum Condition
    {
        [Display(Name = "Oanvänd")]
        UnUsed,

        [Display(Name = "Nyskick")]
        LikeNew,

        [Display(Name = "Gott skick")]
        Good,

        [Display(Name = "Välanvänd")]
        WellUsed,

        [Display(Name = "Defekt")]
        Defect
    }

    public enum AdType
    {
        [Display(Name = "Auktion")]
        Auction,

        [Display(Name = "Köp nu")]
        BuyNow,

        [Display(Name = "Auktion + Köp nu")]
        Both
    }    
}
