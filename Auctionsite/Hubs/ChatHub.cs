using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Auctionsite.Models.Database;

namespace Auctionsite.Hubs
{
    public class ChatHub : Hub // SignalR hub for real-time chat functionality
    {
        private readonly UserManager<User> _userManager;
        public ChatHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // Called from client on load to join specific chat group
        public async Task JoinChatGroup(int chatId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JoinChatGroup failed: {ex.Message}");
                throw;
            }
        }

        // Called from client to join the user's own group
        // This allows the server to send notifications or messages directly to the user. This is used for receiving notifications about new chat messages.
        public async Task JoinUserGroup()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");
            }
        }

        // Called from client when leaving a chat group
        // This is useful for cleanup when a user closes the chat window or navigates away or when the chat is no longer relevant
        public async Task LeaveChatGroup(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
    }
}
