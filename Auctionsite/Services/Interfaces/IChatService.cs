using Auctionsite.Models.Database;

namespace Auctionsite.Services.Interfaces
{
    public interface IChatService
    {
        Task<Chat?> GetChatAsync(int advertisementId, string customerId);
        Task<Chat> CreateChatAsync(int advertisementId, string customerId, string advertiserId);
        Task<Chat> GetOrCreateChatAsync(int advertisementId, string customerId);
        Task<bool> MarkMessageAsReadAsync(int messageId, string userId);

        // Checks if the user has any unread messages
        Task<bool> HasUnreadMessagesAsync(string userId);

        // Checks if the user has any unread messages in a specific conversation
        Task<bool> HasUnreadMessagesInConversationAsync(int chatId, string userId);
        Task<ChatMessage> AddMessageAsync(int chatId, string senderId, string content, bool allowHtml = false);
        Task<Chat?> GetChatByIdAsync(int chatId);
        Task<List<Chat>> GetAllChatsForUserAsync(string userId);
    }
}
