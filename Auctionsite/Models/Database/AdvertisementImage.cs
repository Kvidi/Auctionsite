using System.ComponentModel.DataAnnotations;

namespace Auctionsite.Models.Database
{
    public class AdvertisementImage
    {
        public int Id { get; set; }
        // FK pointing to the Advertisement table
        public int AdvertisementId { get; set; }
        [Required]
        [MaxLength(2048)]
        public string Url { get; set; } = null!;

        // HeadImage
        public bool IsMain { get; set; }

        // General sorting (0 = first, 1 = second...)
        public int Order { get; set; }

        // Making the navigation property nullable so the modelbinder does not complain
        public Advertisement? Advertisement { get; set; } = null!;
    }
}
