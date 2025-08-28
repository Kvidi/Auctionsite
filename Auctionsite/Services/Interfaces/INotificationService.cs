using Auctionsite.Models.Database;
using Auctionsite.Models.VM;

namespace Auctionsite.Services.Interfaces
{
    public interface INotificationService
    {
        // Create notifications for users who have been outbid on an advertisement.
        Task CreateOutbidNotificationsAsync(IEnumerable<string> userIds, int advertisementId, string advertisementTitle, string leadingBidderName, decimal currentHighestBid, string excludingUserId = null);

        // Create a notification for the advertiser when a new leading bid is placed on their advertisement.
        Task CreateNewLeadingBidNotificationAsync(string advertiserId, int advertisementId, string advertisementTitle, decimal newLeadingBid);

        // Create a notification for the bidder when an advertisement they have bid on is expiring today.
        Task<bool> CreateExpiringAdNotificationForBidderAsync(string userId, int adId, string adTitle);

        // Create a notification for the advertiser when their advertisement is expiring today.
        Task<bool> CreateExpiringAdNotificationForAdvertiserAsync(string userId, int adId, string adTitle);

        // Create a notification for saved advertisements that are expiring today.
        Task<bool> CreateExpiringSavedAdsNotificationAsync(string userId, int adCount, int? advertisementId, string? advertisementTitle);

        // Generate notifications for advertisements expiring today. Called once every day when the user loads the site.
        // This method calls the other CreateExpiring-methods and checks for saved ads, user-owned ads, and ads where the user has placed bids.
        Task<bool> GenerateNotificationsAdsExpiringTodayAsync(string userId);

        // Get all notifications for a user and map them to NotificationVM for the notifications sidebar view.
        Task<List<NotificationVM>> GetBidNotificationsAsync(string userId);

        // Mark all notifications as read for a user. Used when the user clicks on the notification icon and opens the notification sidebar.
        Task MarkAllAsReadAsync(string userId);

        // Check if a user has any unread notifications. Used to update the notification icon (show or not show the red dot).
        Task<bool> HasUnreadNotificationsAsync(string userId);

    }
}
