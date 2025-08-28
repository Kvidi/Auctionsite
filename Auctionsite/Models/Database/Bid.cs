using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Auctionsite.Models.Database
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public Advertisement Advertisement { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        [Column(TypeName = "money")]
        [MaxLength(10)]
        public decimal Amount { get; set; }

        public DateTime PlacedAt { get; set; } = DateTime.Now;

        public BidEventType EventType { get; set; } = BidEventType.None;

    }

    // The type of event that this bid represents
    // For use when displaying the bid in the UI
    public enum BidEventType
    {
        None,
        ViaMaxBid,        // Leading user's automatic counterbid after being challenged
        MaxBidReached,    // New bidder exceeds previous max bid entirely
        MaxBidPlacedFirst       // Tie at same max bid amount, but previous bidder keeps lead
    }

}
