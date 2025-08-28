using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctionsite.Models.Database
{
    public class MaxBid
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public Advertisement Advertisement { get; set; } = null!;
        public string UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(10)]
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.Now;
    }
}
