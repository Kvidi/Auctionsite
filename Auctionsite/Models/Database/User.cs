using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Auctionsite.Models.Database
{
    public class User :IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? OrgNr { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public string? DeliveryStreet { get; set; }

        public int? DeliveryZip {  get; set; }

        public string? DeliveryCity { get; set; }

        public string? BillingStreet { get; set; }
        public int? BillingZip { get; set; }
        public string? BillingCity { get; set; }

        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();

        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
        public virtual ICollection<Advertisement> FavouriteAds{ get; set; } = new List<Advertisement>(); // The ads that the user has favourited

        public virtual ICollection<Chat> ChatsAsCustomer { get; set; } = new List<Chat>();
        public virtual ICollection<Chat> ChatsAsAdvertiser { get; set; } = new List<Chat>();

        public ICollection<MaxBid> MaxBids { get; set; } = new List<MaxBid>(); // The max bids for this user
        public ICollection<Bid> Bids { get; set; } = new List<Bid>(); // The automatically incremented, publically displayed, bids for this user
    }
}
