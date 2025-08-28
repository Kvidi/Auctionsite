using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using Auctionsite.Hubs;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IAdService _adService;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<ChatHub> _hubContext; // SignalR hub context for broadcasting messages in real-time

        public ChatController(IChatService chatService, IAdService adService, UserManager<User> userManager, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _adService = adService;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Checks if a chat already exists for this ad and user.
        /// Used when opening the "Kontakta annonsören" popup to decide whether to redirect to the chat view or send the first message.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckExistingChat(int advertisementId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var chat = await _chatService.GetChatAsync(advertisementId, user.Id);

            if (chat != null)
                return Json(new { exists = true, chatId = chat.Id });

            return Json(new { exists = false });
        }

        public async Task<IActionResult> ViewChat(int? chatId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var currentUserId = currentUser.Id;

            var allChats = await _chatService.GetAllChatsForUserAsync(currentUserId);

            Chat? selectedChat = null;

            if (chatId.HasValue)
            {
                selectedChat = await _chatService.GetChatByIdAsync(chatId.Value);

                // Ensure current user is authorized to view this chat
                if (selectedChat == null ||
                    (selectedChat.CustomerId != currentUserId && selectedChat.AdvertiserId != currentUserId))
                {
                    return Forbid();
                }
            }

            var viewModel = new ChatViewModel
            {
                AllChats = allChats,
                SelectedChat = selectedChat,
                CurrentUserId = currentUserId
            };

            return View(viewModel);
        }



        /// <summary>
        /// Handles the very first message sent from the popup.
        /// Creates a chat if needed, and saves the message.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFirstMessage(int advertisementId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Meddelandet kan inte vara tomt.");

            var advertisement = await _adService.GetAdByIdAsync(advertisementId);
            if (advertisement == null)
                return NotFound("Annonsen hittades inte.");

            // Prevent contacting yourself
            if (advertisement.Advertiser.Id == user.Id)
                return BadRequest("Du kan inte skicka ett meddelande till dig själv.");

            // Check if the user is admin
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var chat = await _chatService.GetOrCreateChatAsync(advertisementId, user.Id);
            await _chatService.AddMessageAsync(chat.Id, user.Id, content, allowHtml: isAdmin); // Allow HTML if admin

            // Notify recipient about new message
            await NotifyRecipientNewMessageAsync(chat, user.Id);

            return Ok(new { success = true, chatId = chat.Id });
        }

        /// <summary>
        /// Handles a regular message sent inside an existing chat.
        /// Persists the message and broadcasts it to the SignalR group.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin"); // Check if the user is admin

            var createdMessage = await _chatService.AddMessageAsync(dto.ChatId, user.Id, dto.Content, allowHtml: isAdmin); // Allow HTML if admin

            // Create a broadcast message object to send via SignalR
            var broadcastMessage = new
            {
                Id = createdMessage.Id,
                ChatId = createdMessage.ChatId,
                SenderId = createdMessage.SenderId,
                SenderName = user.UserName,
                Content = createdMessage.Content,
                SentAt = createdMessage.SentAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                IsRead = false
            };

            await _hubContext.Clients.Group($"chat_{dto.ChatId}")
                .SendAsync("ReceiveMessage", broadcastMessage);

            // Notify recipient about new message
            var chat = await _chatService.GetChatByIdAsync(dto.ChatId);
            if (chat != null)
            {
                await NotifyRecipientNewMessageAsync(chat, user.Id);
            }

            return Ok();
        }

        // Marks a specific message as read by the current user and notifies the recipient
        // Also checks read-status for all messages in the chat conversation as well as all messages in all chats, and notifies accordingly
        [HttpPost]
        public async Task<IActionResult> MarkMessageAsRead([FromBody] MarkMessageAsReadRequestDto request)
        {
            if (request == null)
                return BadRequest("Request is null.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _chatService.MarkMessageAsReadAsync(request.MessageId, user.Id);

            if (!success)
                return BadRequest("Message could not be marked as read.");

            // Notify the recipient that this message has been read
            await _hubContext.Clients.Group($"chat_{request.ChatId}")
                .SendAsync("MessageRead", request.MessageId);

            // Check if the chat conversation still has unread messages
            bool chatStillHasUnread = await _chatService.HasUnreadMessagesInConversationAsync(request.ChatId, user.Id);

            if (!chatStillHasUnread)
            {
                // If the chat conversation has no unread messages left, notify the user. Then the dot in the conversations list can be hidden.
                await _hubContext.Clients.Group($"user_{user.Id}")
                    .SendAsync("ChatConversationRead", request.ChatId);
            }

            // Check if user has any unread messages at all left
            var stillHasUnread = await _chatService.HasUnreadMessagesAsync(user.Id);

            if (!stillHasUnread)
            {
                // If the user has no unread messages left, notify the user so the dot by the chat icon can be hidden.
                await _hubContext.Clients.Group($"user_{user.Id}")
                    .SendAsync("AllMessagesRead");
            }

            return Ok();
        }

        /// Checks if the current user has any unread messages across all chats
        public async Task<IActionResult> HasUnreadMessages()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { hasUnread = false });

            var hasUnread = await _chatService.HasUnreadMessagesAsync(userId);

            return Json(new { hasUnread });
        }

        // Notify the recipient of a new message in a chat
        private async Task NotifyRecipientNewMessageAsync(Chat chat, string senderId)
        {
            var receiverId = chat.CustomerId == senderId ? chat.AdvertiserId : chat.CustomerId;

            // Send a notification to the recipient's SignalR group
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendAsync("ReceiveUnreadMessage", chat.Id);
        }
    }

    /// <summary>
    /// DTO used when sending a new message to an existing chat.
    /// DTO stands for Data Transfer Object, which is a simple object used to transfer data between layers.
    /// </summary>
    public class ChatMessageDto
    {
        public int ChatId { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// DTO used when marking a message as read.
    /// </summary>
    public class MarkMessageAsReadRequestDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
    }
}
