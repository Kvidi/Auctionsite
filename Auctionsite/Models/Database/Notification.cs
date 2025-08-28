namespace Auctionsite.Models.Database
{
    public class Notification
    {
        public int Id { get; set; } // Unique identifier for the notification

        public string UserId { get; set; } // Foreign key to the User who receives the notification
        public User User { get; set; } // Navigation property to the User

        public int? AdvertisementId { get; set; } // Optional foreign key to the Advertisement related to the notification. Not used for notifications about multiple advertisements.
        public Advertisement Advertisement { get; set; } // Navigation property to the Advertisement

        public string Message { get; set; } // The content of the notification
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp when the notification was created
        public bool IsRead { get; set; } = false; // Indicates whether the notification has been read by the user

        public NotificationType Type { get; set; } // The type of notification, indicating its purpose or context
    }

    public enum NotificationType 
    {
        Outbid, // User has been outbid on an advertisement
        NewLeadingBid, // Advertiser has a new leading bid on an advertisement
        WatchedAdExpiring, // User's watched advertisement is expiring soon
        BiddedAdExpiring, // User's bidded advertisement is expiring soon
        OwnAdExpiring // Advertiser's advertisement is expiring soon
    }

}
