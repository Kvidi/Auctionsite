using Microsoft.EntityFrameworkCore;
using Auctionsite.Data;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;



        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Create notifications for users who have been outbid on an advertisement.
        // In practice, only the previous leading bidder will receive this notification, so the method can be modified to accept a single user ID.
        // The excludingUserId parameter is used to avoid notifying the user who just placed a new bid, aka the new leading bidder.
        public async Task CreateOutbidNotificationsAsync(
            IEnumerable<string> userIds, int advertisementId, string advertisementTitle, string leadingBidderName, decimal currentHighestBid, string excludingUserId = null)
        {
            var notifications = userIds
                .Where(id => id != excludingUserId)
                .Select(id => new Notification
                {
                    UserId = id,
                    AdvertisementId = advertisementId,
                    Message = $"Du har blivit överbjuden på \"{advertisementTitle}\". Nytt ledande bud är {currentHighestBid:N0} kr.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Type = NotificationType.Outbid
                }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        // Create a notification for the advertiser when a new leading bid is placed on their advertisement.
        public async Task CreateNewLeadingBidNotificationAsync(string advertiserId, int advertisementId, string advertisementTitle, decimal newLeadingBid)
        {
            var notification = new Notification
            {
                UserId = advertiserId,
                AdvertisementId = advertisementId,
                Message = $"Du har fått ett nytt bud på  \"{newLeadingBid:N0} kr\" för {advertisementTitle}.",
                CreatedAt = DateTime.Now,
                IsRead = false,
                Type = NotificationType.NewLeadingBid
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        // Create a notification for the bidder when an advertisement they have bid on is expiring today.
        public async Task<bool> CreateExpiringAdNotificationForBidderAsync(string userId, int adId, string adTitle)
        {
            var today = DateTime.Today;
            string message = $"Du har lagt bud på \"{adTitle}\" som avslutas idag.";

            // If a notification for this ad already exists, do not create a new one.
            bool alreadyExists = await _db.Notifications.AnyAsync(n =>
               n.UserId == userId &&
               n.AdvertisementId == adId &&
               n.Type == NotificationType.BiddedAdExpiring &&
               n.CreatedAt.Date == today.Date
           );

            if (alreadyExists)
                return false;

            var notification = new Notification
            {
                UserId = userId,
                AdvertisementId = adId,
                Message = message,
                CreatedAt = today,
                IsRead = false,
                Type = NotificationType.BiddedAdExpiring
            };

            _db.Notifications.Add(notification);
            return true;
        }

        // Create a notification for the advertiser when their advertisement is expiring today.
        public async Task<bool> CreateExpiringAdNotificationForAdvertiserAsync(string userId, int adId, string adTitle)
        {
            var today = DateTime.Today;
            string message = $"Din annons \"{adTitle}\" avslutas idag.";

            // Return false if a notification for this ad already exists.
            bool alreadyExists = await _db.Notifications.AnyAsync(n =>
                n.UserId == userId &&
                n.AdvertisementId == adId &&
                n.Type == NotificationType.OwnAdExpiring &&
                n.CreatedAt.Date == today.Date
            );

            if (alreadyExists)
                return false;

            var notification = new Notification
            {
                UserId = userId,
                AdvertisementId = adId,
                Message = message,
                CreatedAt = today,
                IsRead = false,
                Type = NotificationType.OwnAdExpiring
            };

            _db.Notifications.Add(notification);
            return true;
        }

        // Create a notification for saved advertisements that are expiring today.
        // If there are multiple saved ads expiring, a single notification is created, summarizing the count.
        // advertisementId and advertisementTitle are used only if there is only one ad expiring.
        public async Task<bool> CreateExpiringSavedAdsNotificationAsync(string userId, int adCount, int? advertisementId, string? advertisementTitle)
        {
            var today = DateTime.Today;

            // Depending on the number of ads, create a different message.
            string message = adCount > 1
                ? $"Du har \"{adCount} sparade annonser\" som avslutas idag."
                : $"Du har en sparad annons som avslutas idag: \"{advertisementTitle}\"";

            // Return false if a notification with the same message already exists for today.
            bool alreadyExists = await _db.Notifications.AnyAsync(n =>
                n.UserId == userId &&
                n.Message == message &&
                n.CreatedAt.Date == today
            );

            if (alreadyExists)
                return false;

            // Remove old notifications for today.
            var oldSavedAdNotifications = await _db.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    n.CreatedAt.Date == today &&
                    n.Type == NotificationType.WatchedAdExpiring)
                .ToListAsync();

            if (oldSavedAdNotifications.Any())
            {
                _db.Notifications.RemoveRange(oldSavedAdNotifications);
            }

          
            var notification = new Notification
            {
                UserId = userId,
                AdvertisementId = adCount > 1 ? null : advertisementId,
                Message = message,
                CreatedAt = today,
                IsRead = false,
                Type = NotificationType.WatchedAdExpiring
            };

            _db.Notifications.Add(notification);
            return true;
        }

        // Generate notifications for advertisements expiring today. Called once every day when the user loads the site.
        // This method calls the other CreateExpiring-methods and checks for saved ads, user-owned ads, and ads where the user has placed bids.
        public async Task<bool> GenerateNotificationsAdsExpiringTodayAsync(string userId)
        {
            bool anyCreated = false; // To track if any notifications were created. Used to know whether the user should be notified.
            var today = DateTime.Today;

            // Remove old notifications for expiring ads that are not from today.
            var oldNotifications = await _db.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    n.CreatedAt.Date != today && (
                        n.Type == NotificationType.WatchedAdExpiring ||
                        n.Type == NotificationType.OwnAdExpiring ||
                        n.Type == NotificationType.BiddedAdExpiring
                    )
                )
                .ToListAsync();

            if (oldNotifications.Any())
            {
                _db.Notifications.RemoveRange(oldNotifications);
            }

            // Get advertisements that the user has saved and are expiring today.
            var expiringSavedAds = await _db.Advertisements
                .Where(ad => ad.UsersWhoFavourited.Any(u => u.Id == userId) && ad.AuctionEndDate.Value.Date == today.Date)
                .ToListAsync();

            if (expiringSavedAds.Any())
            {
                // If there are saved ads expiring today, create a notification for the user.

                int adCount = expiringSavedAds.Count;
                int? advertisementId = adCount == 1 ? expiringSavedAds[0].Id : null; // Only set if there is exactly one ad expiring.
                string? advertisementTitle = adCount == 1 ? expiringSavedAds[0].Title : null; // Only set if there is exactly one ad expiring.

                // The method returns true if a notification was created (false if there already was one created).
                bool savedNotificationCreated = await CreateExpiringSavedAdsNotificationAsync(userId, adCount, advertisementId, advertisementTitle);

                // If a notification was created for saved ads, set anyCreated to true.
                if (savedNotificationCreated)
                    anyCreated = true;
            }

            // Get advertisements created by the user that are expiring today.
            var userAds = await _db.Advertisements
                .Where(ad => ad.Advertiser.Id == userId && ad.AuctionEndDate.Value.Date == today.Date)
                .ToListAsync();

            // If there are user-owned ads expiring today, create notifications for each.
            // The method returns true if a notification was created (false if there already was one created).
            foreach (var ad in userAds)
            {
                bool advertiserNotificationCreated = await CreateExpiringAdNotificationForAdvertiserAsync(userId, ad.Id, ad.Title);

                if (advertiserNotificationCreated)
                    anyCreated = true;
            }

            // Get advertisements where the user has placed bids and that are expiring today.
            var biddedAds = await _db.Bids
                .Where(b => b.UserId == userId && b.Advertisement.AuctionEndDate.Value.Date == today.Date)
                .Select(b => b.Advertisement)
                .Distinct()
                .ToListAsync();

            // If there are ads where the user has placed bids expiring today, create notifications for each.
            // The method returns true if a notification was created (false if there already was one created).
            foreach (var ad in biddedAds)
            {
                bool biddedAdNotificationCreated = await CreateExpiringAdNotificationForBidderAsync(userId, ad.Id, ad.Title);

                if (biddedAdNotificationCreated)
                    anyCreated = true;
            }

            // Save changes to the database and return whether any notifications were created.
            await _db.SaveChangesAsync();
            return anyCreated;
        }

        // Get all notifications for a user and map them to NotificationVM for the notifications sidebar view.
        // Ordered by the most recent first (descending order by Id).
        public async Task<List<NotificationVM>> GetBidNotificationsAsync(string userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Advertisement)
                .ThenInclude(n => n.Images)
                .OrderByDescending(n => n.Id)
                .ToListAsync();

            var notificationsVM = notifications
                .Select(n  => new NotificationVM
                {
                    NotificationId = n.Id,
                    AdvertisementId = n.AdvertisementId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    ImageUrl = n.Advertisement.Images.Where(img => img.IsMain).Select(img => img.Url).FirstOrDefault() ?? string.Empty,
                    IsRead = n.IsRead
                })
                .ToList();

            return notificationsVM;
        }

        // Mark all notifications as read for a user. Used when the user clicks on the notification icon and opens the notification sidebar.
        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _db.SaveChangesAsync();
        }

        // Check if a user has any unread notifications. Used to update the notification icon (show or not show the red dot).
        public async Task<bool> HasUnreadNotificationsAsync(string userId)
        {
            return await _db.Notifications
                .AnyAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
