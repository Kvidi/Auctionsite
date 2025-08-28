using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Components
{
    // Backend view component for the notifications sidebar
    public class NotificationSidebarViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public NotificationSidebarViewComponent(INotificationService notificationService, UserManager<User> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the notifications for the logged-in user and returns the view component.
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return View("Default", new List<NotificationVM>());

            var notifications = await _notificationService.GetBidNotificationsAsync(user.Id);

            return View("Default", notifications);
        }
    }
}
