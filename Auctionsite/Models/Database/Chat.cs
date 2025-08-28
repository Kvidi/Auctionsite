namespace Auctionsite.Models.Database
{
    public class Chat
    {
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public virtual Advertisement Advertisement { get; set; }

        public string CustomerId { get; set; }
        public virtual User Customer { get; set; }

        public string AdvertiserId { get; set; }
        public virtual User Advertiser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    }

}
