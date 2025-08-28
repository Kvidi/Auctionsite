using Microsoft.AspNetCore.SignalR;

namespace Auctionsite.Hubs
{
    // AdvertisementHub allows clients to join and leave advertisement groups for real-time updates
    // Used to receive updates about bids on advertisements
    public class AdvertisementHub : Hub
    {
        public async Task JoinGroup(string adGroupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, adGroupId);
        }

        public async Task LeaveGroup(string adGroupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, adGroupId);
        }
    }
}
