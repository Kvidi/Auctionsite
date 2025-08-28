using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Auctionsite.Data;
using Auctionsite.Models.Database;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _db;

        public ChatService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get a chat between customer and advertiser for a specific advertisement
        public async Task<Chat?> GetChatAsync(int advertisementId, string customerId)
        {
            return await _db.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.AdvertisementId == advertisementId &&
                    c.CustomerId == customerId);
        }

        // Create a new chat
        public async Task<Chat> CreateChatAsync(int advertisementId, string customerId, string advertiserId)
        {
            var chat = new Chat
            {
                AdvertisementId = advertisementId,
                CustomerId = customerId,
                AdvertiserId = advertiserId,
            };

            _db.Chats.Add(chat);
            await _db.SaveChangesAsync();

            return chat;
        }

        // Get or create a chat, depending on whether it already exists
        public async Task<Chat> GetOrCreateChatAsync(int advertisementId, string customerId)
        {
            var existingChat = await GetChatAsync(advertisementId, customerId);
            if (existingChat != null)
                return existingChat;

            var advertisement = await _db.Advertisements
                .Include(a => a.Advertiser)
                .FirstOrDefaultAsync(a => a.Id == advertisementId);

            if (advertisement == null)
                throw new Exception("Advertisement not found");

            return await CreateChatAsync(advertisementId, customerId, advertisement.Advertiser.Id);
        }

        // Marks a message as read if it was previously unread and not sent by the current user
        public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
        {
            var message = await _db.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId != userId && !m.IsRead);

            if (message == null)
                return false;

            message.IsRead = true;
            await _db.SaveChangesAsync();

            return true;
        }

        // Checks if the user has any unread messages
        public Task<bool> HasUnreadMessagesAsync(string userId)
        {
            var hasUnreadMessages = _db.ChatMessages
                .AnyAsync(m => m.SenderId != userId && !m.IsRead);

            return hasUnreadMessages;
        }

        // Checks if the user has any unread messages in a specific conversation
        // This is used to determine if the user should be notified about new messages in a specific chat
        public Task<bool> HasUnreadMessagesInConversationAsync(int chatId, string userId)
        {
            var hasUnreadMessages = _db.ChatMessages
                .AnyAsync(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead);
            return hasUnreadMessages;
        }

        // Adds a new message to the specified chat and returns the created message
        public async Task<ChatMessage> AddMessageAsync(int chatId, string senderId, string content, bool allowHtml = false)
        {
            string finalContent;

            if (allowHtml)
            {
                // --- Allow admin to use HTML-tags ---
                finalContent = content;
            }
            else
            {
                // --- Do not allow other users to use HTML-tags ---                               
                finalContent = Regex.Replace(content, "<.*?>", string.Empty); // Strip any HTML tags but preserve Unicode characters 
            }
            var message = new ChatMessage
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = finalContent,
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            return message;
        }

        // Get chat by ID
        public async Task<Chat?> GetChatByIdAsync(int chatId)
        {
            return await _db.Chats
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Customer)
                .Include(c => c.Advertiser)
                .Include(c => c.Advertisement)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        // Get all chats for a specific user (customer or advertiser)
        public async Task<List<Chat>> GetAllChatsForUserAsync(string userId)
        {
            return await _db.Chats
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Advertisement)
                .Include(c => c.Customer)
                .Include(c => c.Advertiser)
                .Where(c => c.CustomerId == userId || c.AdvertiserId == userId)
                .ToListAsync();
        }
    }
}
