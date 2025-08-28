using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Security.Claims;
using Auctionsite.Data;
using Auctionsite.Models;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly UserManager<User> _userManager;
    private readonly INotificationService _notificationService;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager, INotificationService notificationService)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        var homeVM = new HomeVM
        {
            LastChanceAds = _applicationDbContext.Advertisements
                .Where(ad => ad.AuctionEndDate.HasValue && ad.AuctionEndDate.Value > DateTime.Now && ad.ApprovedAt != null)
                .OrderBy(ad => ad.AuctionEndDate)
                .Take(5)
                .Select(ad => new AdCardVM
                {
                    Id = ad.Id,
                    Title = ad.Title,
                    HeadImageUrl = ad.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.Url)
                        .FirstOrDefault() ?? string.Empty,
                    AuctionEndDate = ad.AuctionEndDate,
                    AdType = ad.AdType,
                    StartingPrice = ad.StartingPrice,
                    BuyNowPrice = ad.BuyNowPrice,
                    CurrentHighestBid = ad.CurrentHighestBid,
                    BidCount = ad.Bids != null ? ad.Bids.Count : 0,
                    IsFavourite = userId != null && ad.UsersWhoFavourited.Any(u => u.Id == userId),
                    ApprovedAt = ad.ApprovedAt
                })
                .ToList(),
            NewAds = _applicationDbContext.Advertisements
                .Where(ad => ad.AuctionEndDate.HasValue && ad.AuctionEndDate.Value > DateTime.Now && ad.ApprovedAt != null)
                .OrderByDescending(ad => ad.ApprovedAt)
                .Take(5)
                .Select(ad => new AdCardVM
                {
                    Id = ad.Id,
                    Title = ad.Title,
                    HeadImageUrl = ad.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.Url)
                        .FirstOrDefault() ?? string.Empty,
                    AuctionEndDate = ad.AuctionEndDate,
                    AdType = ad.AdType,
                    StartingPrice = ad.StartingPrice,
                    BuyNowPrice = ad.BuyNowPrice,
                    CurrentHighestBid = ad.CurrentHighestBid,
                    BidCount = ad.Bids != null ? ad.Bids.Count : 0,
                    IsFavourite = userId != null && ad.UsersWhoFavourited.Any(u => u.Id == userId),
                    ApprovedAt = ad.ApprovedAt
                })
                .ToList(),
            PopularAds = _applicationDbContext.Advertisements
                .Where(ad => ad.AuctionEndDate.HasValue && ad.AuctionEndDate.Value > DateTime.Now && ad.ApprovedAt != null)
                .OrderByDescending(ad => ad.ViewCount)
                .Take(5)
                .Select(ad => new AdCardVM
                {
                    Id = ad.Id,
                    Title = ad.Title,
                    HeadImageUrl = ad.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.Url)
                        .FirstOrDefault() ?? string.Empty,
                    AuctionEndDate = ad.AuctionEndDate,
                    AdType = ad.AdType,
                    StartingPrice = ad.StartingPrice,
                    BuyNowPrice = ad.BuyNowPrice,
                    CurrentHighestBid = ad.CurrentHighestBid,
                    BidCount = ad.Bids != null ? ad.Bids.Count : 0,
                    IsFavourite = userId != null && ad.UsersWhoFavourited.Any(u => u.Id == userId),
                    ApprovedAt = ad.ApprovedAt
                })
                .ToList(),
            SearchFilter = new AdSearchFilterViewModel()
        };
        return View(homeVM);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Endpoint to load the notification sidebar
    public IActionResult LoadNotificationSidebar()
    {
        return ViewComponent("NotificationSidebar");
    }

    // Endpoint to mark all notifications as read
    // This is used when the user clicks on the notification icon and opens the notification sidebar
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllAsReadAsync(userId);

        return Ok();
    }

    // Endpoint to check if the user has any unread notifications
    // Used to update the notification icon (show or not show the red dot)
    [HttpGet]
    public async Task<IActionResult> HasUnreadNotifications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var hasUnread = await _notificationService.HasUnreadNotificationsAsync(user.Id);
        return Json(new { hasUnread });
    }

}
