using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class BidHistoryVM
    {
        public string UserName { get; set; }

        [Column(TypeName = "money")]
        [MaxLength(10)]
        public decimal Amount { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.Now;
        public BidEventType EventType { get; set; } = BidEventType.None;
    }
}
