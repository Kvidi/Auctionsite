namespace Auctionsite.Models.Database
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public string SenderId { get; set; }
        public virtual User Sender { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
