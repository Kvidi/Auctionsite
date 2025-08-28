using Auctionsite.Models.Database;

namespace Auctionsite.Models.VM
{
    public class ChatViewModel
    {
        public List<Chat> AllChats { get; set; } = new List<Chat>();
        public Chat? SelectedChat { get; set; }
        public string CurrentUserId { get; set; }
    }
}
