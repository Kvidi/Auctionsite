using System.ComponentModel.DataAnnotations;
using Auctionsite.Models.Database;

namespace Auctionsite.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relationships
        public string ReviewerId { get; set; }
        public User Reviewer { get; set; }

        public string TargetUserId { get; set; } // the one being reviewed
        public User TargetUser { get; set; }
    }
}
