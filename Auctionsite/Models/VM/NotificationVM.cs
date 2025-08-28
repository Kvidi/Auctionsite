namespace Auctionsite.Models.VM
{
    public class NotificationVM
    {
        public int NotificationId { get; set; }
        public int? AdvertisementId { get; set; } // Nullable to allow notifications not related to singular ads (i e for multiple ads, specifically saved ads)
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; } // Thumbnail image
        public bool IsRead { get; set; } // Indicates if the notification has been read
    }
}
